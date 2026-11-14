using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace BUS.ImportExport;

/// <summary>
/// Bộ đọc/ghi tệp Excel .xls không phụ thuộc thư viện ngoài:
/// - Ghi chuẩn nhị phân Excel 97-2003 (OLE2 Compound Document + BIFF8) hỗ trợ tiếng Việt UTF-16LE.
/// - Đọc được cả .xls nhị phân BIFF8, Excel 2003 XML (SpreadsheetML), HTML table .xls và CSV/TSV .xls.
/// </summary>
internal static partial class XlsWorkbookCodec
{
    private const uint _free_sector = 0xFFFFFFFF;
    private const uint _end_of_chain = 0xFFFFFFFE;
    private const uint _fat_sector = 0xFFFFFFFD;
    private const uint _difat_sector = 0xFFFFFFFC;
    private static readonly byte[] _ole2_magic = [0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1];

    public static void WriteXls(string file_path, string sheet_name, IReadOnlyList<IReadOnlyList<object?>> rows)
    {
        byte[] workbook_stream = BuildBiff8Workbook(sheet_name, rows);
        byte[] compound_bytes = WrapInOle2Container(workbook_stream);
        File.WriteAllBytes(file_path, compound_bytes);
    }

    public static List<List<string>> ReadXlsRows(string file_path)
    {
        byte[] raw_bytes = File.ReadAllBytes(file_path);
        if (raw_bytes.Length == 0)
            return [];

        if (raw_bytes.Length >= 8 && raw_bytes.AsSpan(0, 8).SequenceEqual(_ole2_magic))
        {
            byte[] workbook_stream = ExtractWorkbookStreamFromOle2(raw_bytes);
            return ParseBiff8Workbook(workbook_stream);
        }

        string text = DecodeTextWithBom(raw_bytes);
        string trimmed = text.TrimStart();

        if (trimmed.StartsWith("<?xml", StringComparison.OrdinalIgnoreCase)
            || trimmed.StartsWith("<Workbook", StringComparison.OrdinalIgnoreCase))
        {
            return ParseSpreadsheetMlXml(text);
        }

        if (trimmed.Contains("<table", StringComparison.OrdinalIgnoreCase)
            && trimmed.Contains("<tr", StringComparison.OrdinalIgnoreCase))
        {
            return ParseHtmlTable(text);
        }

        return ParseDelimitedText(text);
    }

    private static byte[] BuildBiff8Workbook(string sheet_name, IReadOnlyList<IReadOnlyList<object?>> rows)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.Unicode, leaveOpen: true);

        // 1. Globals Substream
        WriteBof(writer, 0x0005);
        WriteRecord(writer, 0x00E1, BitConverter.GetBytes((ushort)0x04B0)); // INTERFACEHDR
        WriteRecord(writer, 0x00C1, new byte[2]); // MMS
        WriteRecord(writer, 0x00E2, []); // INTERFACEEND
        WriteRecord(writer, 0x0042, BitConverter.GetBytes((ushort)1200)); // CODEPAGE UTF-16LE
        WriteRecord(writer, 0x0161, new byte[2]); // DSF
        WriteWindow1(writer);
        WriteDefaultFonts(writer);
        WriteDefaultXfs(writer);
        WriteNormalStyle(writer);

        string safe_sheet = string.IsNullOrWhiteSpace(sheet_name) ? "NhanVien" : sheet_name.Trim();
        if (safe_sheet.Length > 31)
            safe_sheet = safe_sheet[..31];

        // Giữ vị trí offset để điền địa chỉ BOF của sheet sau khi ghi xong Globals.
        byte[] sheet_name_bytes = Encoding.Unicode.GetBytes(safe_sheet);
        long boundsheet_payload_pos = stream.Position + 4;
        using (var bs_ms = new MemoryStream())
        using (var bs_bw = new BinaryWriter(bs_ms))
        {
            bs_bw.Write((uint)0); // lbPlyPos placeholder
            bs_bw.Write((ushort)0); // grbit visible worksheet
            bs_bw.Write((byte)safe_sheet.Length);
            bs_bw.Write((byte)0x01); // UTF-16LE
            bs_bw.Write(sheet_name_bytes);
            WriteRecord(writer, 0x0085, bs_ms.ToArray()); // BOUNDSHEET
        }

        WriteRecord(writer, 0x000A, []); // EOF Globals

        uint sheet_bof_offset = (uint)stream.Position;
        long saved_end = stream.Position;
        stream.Position = boundsheet_payload_pos;
        writer.Write(sheet_bof_offset);
        stream.Position = saved_end;

        // 2. Worksheet Substream
        WriteBof(writer, 0x0010);

        int max_cols = 0;
        foreach (IReadOnlyList<object?> row in rows)
        {
            if (row.Count > max_cols)
                max_cols = row.Count;
        }

        using (var dim_ms = new MemoryStream())
        using (var dim_bw = new BinaryWriter(dim_ms))
        {
            dim_bw.Write((uint)0);
            dim_bw.Write((uint)rows.Count);
            dim_bw.Write((ushort)0);
            dim_bw.Write((ushort)max_cols);
            dim_bw.Write((ushort)0);
            WriteRecord(writer, 0x0200, dim_ms.ToArray()); // DIMENSIONS
        }

        for (int row_index = 0; row_index < rows.Count; row_index++)
        {
            IReadOnlyList<object?> row = rows[row_index];
            for (int col_index = 0; col_index < row.Count; col_index++)
            {
                object? value = row[col_index];
                if (value is null)
                    continue;

                if (value is decimal dec_val)
                    WriteNumberCell(writer, (ushort)row_index, (ushort)col_index, (double)dec_val);
                else if (value is double dbl_val)
                    WriteNumberCell(writer, (ushort)row_index, (ushort)col_index, dbl_val);
                else if (value is int int_val)
                    WriteNumberCell(writer, (ushort)row_index, (ushort)col_index, int_val);
                else if (value is DateTime dt_val)
                    WriteLabelCell(writer, (ushort)row_index, (ushort)col_index, dt_val.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture));
                else
                    WriteLabelCell(writer, (ushort)row_index, (ushort)col_index, value.ToString() ?? string.Empty);
            }
        }

        WriteWindow2(writer);
        WriteRecord(writer, 0x000A, []); // EOF Worksheet

        // Đảm bảo kích thước >= 4096 byte và chia hết cho 512 để không cần MiniStream trong OLE2.
        int min_size = Math.Max(4096, (int)stream.Length);
        int padded_size = ((min_size + 511) / 512) * 512;
        if (padded_size > stream.Length)
            writer.Write(new byte[padded_size - stream.Length]);

        writer.Flush();
        return stream.ToArray();
    }

    private static void WriteBof(BinaryWriter writer, ushort substream_type)
    {
        byte[] payload = new byte[16];
        BitConverter.GetBytes((ushort)0x0600).CopyTo(payload, 0); // BIFF8
        BitConverter.GetBytes(substream_type).CopyTo(payload, 2);
        BitConverter.GetBytes((ushort)0x0DBB).CopyTo(payload, 4);
        BitConverter.GetBytes((ushort)0x07CC).CopyTo(payload, 6);
        WriteRecord(writer, 0x0809, payload);
    }

    private static void WriteWindow1(BinaryWriter writer)
    {
        byte[] payload = new byte[18];
        BitConverter.GetBytes((ushort)0).CopyTo(payload, 0);
        BitConverter.GetBytes((ushort)0).CopyTo(payload, 2);
        BitConverter.GetBytes((ushort)15000).CopyTo(payload, 4);
        BitConverter.GetBytes((ushort)9000).CopyTo(payload, 6);
        BitConverter.GetBytes((ushort)0x0038).CopyTo(payload, 8);
        BitConverter.GetBytes((ushort)1).CopyTo(payload, 14);
        BitConverter.GetBytes((ushort)600).CopyTo(payload, 16);
        WriteRecord(writer, 0x003D, payload);
    }

    private static void WriteWindow2(BinaryWriter writer)
    {
        byte[] payload = new byte[18];
        BitConverter.GetBytes((ushort)0x06B6).CopyTo(payload, 0);
        BitConverter.GetBytes((ushort)0x0040).CopyTo(payload, 6);
        WriteRecord(writer, 0x023E, payload);
    }

    private static void WriteDefaultFonts(BinaryWriter writer)
    {
        // Font chuẩn (Calibri / Arial 10pt) để các trình đọc Excel ánh xạ đúng XF.
        byte[] name_bytes = Encoding.Unicode.GetBytes("Arial");
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write((ushort)200); // 10pt
        bw.Write((ushort)0);
        bw.Write((ushort)0x7FFF);
        bw.Write((ushort)400); // regular
        bw.Write((ushort)0);
        bw.Write((byte)0);
        bw.Write((byte)0);
        bw.Write((byte)0);
        bw.Write((byte)0);
        bw.Write((byte)"Arial".Length);
        bw.Write((byte)0x01);
        bw.Write(name_bytes);
        byte[] font_payload = ms.ToArray();
        for (int index = 0; index < 4; index++)
            WriteRecord(writer, 0x0031, font_payload);
    }

    private static void WriteDefaultXfs(BinaryWriter writer)
    {
        byte[] style_xf = new byte[20];
        style_xf[4] = 0xF5;
        style_xf[5] = 0xFF;
        style_xf[6] = 0x20;
        style_xf[16] = 0xC0;
        style_xf[17] = 0x20;

        for (int index = 0; index < 15; index++)
            WriteRecord(writer, 0x00E0, style_xf);

        byte[] cell_xf = new byte[20];
        cell_xf[4] = 0x01;
        cell_xf[5] = 0x00;
        cell_xf[6] = 0x20;
        cell_xf[16] = 0xC0;
        cell_xf[17] = 0x20;
        WriteRecord(writer, 0x00E0, cell_xf);
    }

    private static void WriteNormalStyle(BinaryWriter writer)
    {
        byte[] payload = [0x00, 0x80, 0x00, 0xFF];
        WriteRecord(writer, 0x0293, payload);
    }

    private static void WriteNumberCell(BinaryWriter writer, ushort row, ushort col, double value)
    {
        using var ms = new MemoryStream(14);
        using var bw = new BinaryWriter(ms);
        bw.Write(row);
        bw.Write(col);
        bw.Write((ushort)15);
        bw.Write(value);
        WriteRecord(writer, 0x0203, ms.ToArray());
    }

    private static void WriteLabelCell(BinaryWriter writer, ushort row, ushort col, string text)
    {
        string safe = text.Length > 255 ? text[..255] : text;
        byte[] utf16 = Encoding.Unicode.GetBytes(safe);
        using var ms = new MemoryStream(9 + utf16.Length);
        using var bw = new BinaryWriter(ms);
        bw.Write(row);
        bw.Write(col);
        bw.Write((ushort)15);
        bw.Write((ushort)safe.Length);
        bw.Write((byte)0x01); // UTF-16LE
        bw.Write(utf16);
        WriteRecord(writer, 0x0204, ms.ToArray());
    }

    private static void WriteRecord(BinaryWriter writer, ushort record_id, byte[] payload)
    {
        writer.Write(record_id);
        writer.Write((ushort)payload.Length);
        writer.Write(payload);
    }

    private static byte[] WrapInOle2Container(byte[] workbook_stream)
    {
        const int sector_size = 512;
        int wb_sectors = workbook_stream.Length / sector_size;
        int fat_sectors = Math.Max(1, (1 + wb_sectors + 127) / 128);
        while (fat_sectors * 128 < 1 + fat_sectors + wb_sectors)
            fat_sectors++;

        int wb_first_sector = 1 + fat_sectors;
        using var output = new MemoryStream();
        using var bw = new BinaryWriter(output);

        // OLE2 Header (512 bytes)
        byte[] header = new byte[sector_size];
        _ole2_magic.CopyTo(header, 0);
        BitConverter.GetBytes((ushort)0x003E).CopyTo(header, 0x18);
        BitConverter.GetBytes((ushort)0x0003).CopyTo(header, 0x1A);
        BitConverter.GetBytes((ushort)0xFFFE).CopyTo(header, 0x1C);
        BitConverter.GetBytes((ushort)9).CopyTo(header, 0x1E);
        BitConverter.GetBytes((ushort)6).CopyTo(header, 0x20);
        BitConverter.GetBytes((uint)fat_sectors).CopyTo(header, 0x2C);
        BitConverter.GetBytes((uint)0).CopyTo(header, 0x30); // Directory at sector 0
        BitConverter.GetBytes((uint)4096).CopyTo(header, 0x38);
        BitConverter.GetBytes(_end_of_chain).CopyTo(header, 0x3C);
        BitConverter.GetBytes((uint)0).CopyTo(header, 0x40);
        BitConverter.GetBytes(_end_of_chain).CopyTo(header, 0x44);
        BitConverter.GetBytes((uint)0).CopyTo(header, 0x48);

        for (int index = 0; index < 109; index++)
        {
            uint val = index < fat_sectors ? (uint)(1 + index) : _free_sector;
            BitConverter.GetBytes(val).CopyTo(header, 0x4C + index * 4);
        }
        bw.Write(header);

        // Sector 0: Directory Sector (4 entries x 128 bytes)
        byte[] dir_sector = new byte[sector_size];
        WriteDirectoryEntry(dir_sector, 0, "Root Entry", 5, _end_of_chain, 0, child_id: 1);
        WriteDirectoryEntry(dir_sector, 128, "Workbook", 2, (uint)wb_first_sector, (uint)workbook_stream.Length, child_id: -1);
        for (int slot = 2; slot < 4; slot++)
        {
            int offset = slot * 128;
            BitConverter.GetBytes(-1).CopyTo(dir_sector, offset + 0x44);
            BitConverter.GetBytes(-1).CopyTo(dir_sector, offset + 0x48);
            BitConverter.GetBytes(-1).CopyTo(dir_sector, offset + 0x4C);
        }
        bw.Write(dir_sector);

        // Sectors 1..fat_sectors: FAT
        int total_fat_slots = fat_sectors * 128;
        uint[] fat = new uint[total_fat_slots];
        Array.Fill(fat, _free_sector);
        fat[0] = _end_of_chain;
        for (int index = 0; index < fat_sectors; index++)
            fat[1 + index] = _fat_sector;
        for (int index = 0; index < wb_sectors; index++)
            fat[wb_first_sector + index] = index == wb_sectors - 1 ? _end_of_chain : (uint)(wb_first_sector + index + 1);

        foreach (uint entry in fat)
            bw.Write(entry);

        // Sectors wb_first_sector..: Workbook stream
        bw.Write(workbook_stream);
        bw.Flush();
        return output.ToArray();
    }

    private static void WriteDirectoryEntry(byte[] buffer, int offset, string name, byte object_type,
        uint start_sector, uint stream_size, int child_id)
    {
        byte[] name_bytes = Encoding.Unicode.GetBytes(name);
        name_bytes.CopyTo(buffer, offset);
        BitConverter.GetBytes((ushort)(name_bytes.Length + 2)).CopyTo(buffer, offset + 0x40);
        buffer[offset + 0x42] = object_type;
        buffer[offset + 0x43] = 1; // black node
        BitConverter.GetBytes(-1).CopyTo(buffer, offset + 0x44); // left sibling
        BitConverter.GetBytes(-1).CopyTo(buffer, offset + 0x48); // right sibling
        BitConverter.GetBytes(child_id).CopyTo(buffer, offset + 0x4C);
        BitConverter.GetBytes(start_sector).CopyTo(buffer, offset + 0x74);
        BitConverter.GetBytes(stream_size).CopyTo(buffer, offset + 0x78);
    }

    private static byte[] ExtractWorkbookStreamFromOle2(byte[] data)
    {
        int sector_shift = BitConverter.ToUInt16(data, 0x1E);
        int mini_shift = BitConverter.ToUInt16(data, 0x20);
        int sector_size = 1 << sector_shift;
        int mini_sector_size = 1 << mini_shift;
        uint fat_count = BitConverter.ToUInt32(data, 0x2C);
        uint first_dir_sector = BitConverter.ToUInt32(data, 0x30);
        uint mini_cutoff = BitConverter.ToUInt32(data, 0x38);
        uint first_minifat_sector = BitConverter.ToUInt32(data, 0x3C);
        uint first_difat_sector = BitConverter.ToUInt32(data, 0x44);

        var fat_sector_ids = new List<uint>();
        for (int index = 0; index < 109 && fat_sector_ids.Count < fat_count; index++)
        {
            uint sector_id = BitConverter.ToUInt32(data, 0x4C + index * 4);
            if (sector_id != _free_sector)
                fat_sector_ids.Add(sector_id);
        }

        uint difat_cursor = first_difat_sector;
        int entries_per_sector = sector_size / 4;
        while (difat_cursor != _end_of_chain && difat_cursor != _free_sector && fat_sector_ids.Count < fat_count)
        {
            int pos = 512 + (int)difat_cursor * sector_size;
            if (pos + sector_size > data.Length)
                break;

            for (int index = 0; index < entries_per_sector - 1 && fat_sector_ids.Count < fat_count; index++)
            {
                uint sector_id = BitConverter.ToUInt32(data, pos + index * 4);
                if (sector_id != _free_sector)
                    fat_sector_ids.Add(sector_id);
            }
            difat_cursor = BitConverter.ToUInt32(data, pos + (entries_per_sector - 1) * 4);
        }

        var fat = new List<uint>(fat_sector_ids.Count * entries_per_sector);
        foreach (uint sector_id in fat_sector_ids)
        {
            int pos = 512 + (int)sector_id * sector_size;
            if (pos + sector_size > data.Length)
                continue;
            for (int index = 0; index < entries_per_sector; index++)
                fat.Add(BitConverter.ToUInt32(data, pos + index * 4));
        }

        byte[] dir_bytes = ReadChain(data, fat, first_dir_sector, sector_size, int.MaxValue);
        byte[]? mini_stream = null;
        var minifat = new List<uint>();
        if (first_minifat_sector != _end_of_chain && first_minifat_sector != _free_sector)
        {
            byte[] minifat_bytes = ReadChain(data, fat, first_minifat_sector, sector_size, int.MaxValue);
            for (int pos = 0; pos + 4 <= minifat_bytes.Length; pos += 4)
                minifat.Add(BitConverter.ToUInt32(minifat_bytes, pos));
        }

        uint root_start = BitConverter.ToUInt32(dir_bytes, 0x74);
        int root_size = (int)Math.Min(int.MaxValue, BitConverter.ToUInt32(dir_bytes, 0x78));
        if (root_start != _end_of_chain && root_start != _free_sector && root_size > 0)
            mini_stream = ReadChain(data, fat, root_start, sector_size, root_size);

        for (int entry_offset = 0; entry_offset + 128 <= dir_bytes.Length; entry_offset += 128)
        {
            ushort name_len = BitConverter.ToUInt16(dir_bytes, entry_offset + 0x40);
            byte type = dir_bytes[entry_offset + 0x42];
            if (type != 2 || name_len < 2 || name_len > 64)
                continue;

            string entry_name = Encoding.Unicode.GetString(dir_bytes, entry_offset, name_len - 2);
            if (!entry_name.Equals("Workbook", StringComparison.OrdinalIgnoreCase)
                && !entry_name.Equals("Book", StringComparison.OrdinalIgnoreCase))
                continue;

            uint start = BitConverter.ToUInt32(dir_bytes, entry_offset + 0x74);
            long size = BitConverter.ToUInt32(dir_bytes, entry_offset + 0x78);
            if (size < mini_cutoff && mini_stream is not null && minifat.Count > 0)
                return ReadMiniChain(mini_stream, minifat, start, mini_sector_size, (int)size);

            return ReadChain(data, fat, start, sector_size, (int)size);
        }

        throw new InvalidDataException("Tệp .xls không chứa luồng dữ liệu Workbook hợp lệ.");
    }

    private static byte[] ReadChain(byte[] data, List<uint> fat, uint start_sector, int sector_size, int max_bytes)
    {
        using var ms = new MemoryStream();
        uint cursor = start_sector;
        int guard = 0;
        while (cursor < fat.Count && cursor != _end_of_chain && cursor != _free_sector && guard++ < 1_000_000)
        {
            int pos = 512 + (int)cursor * sector_size;
            if (pos + sector_size <= data.Length)
                ms.Write(data, pos, sector_size);
            else if (pos < data.Length)
                ms.Write(data, pos, data.Length - pos);

            if (ms.Length >= max_bytes)
                break;
            cursor = fat[(int)cursor];
        }

        byte[] result = ms.ToArray();
        if (max_bytes < result.Length)
            Array.Resize(ref result, max_bytes);
        return result;
    }

    private static byte[] ReadMiniChain(byte[] mini_stream, List<uint> minifat, uint start_sector, int mini_sector_size, int length)
    {
        using var ms = new MemoryStream(length);
        uint cursor = start_sector;
        int guard = 0;
        while (cursor < minifat.Count && cursor != _end_of_chain && cursor != _free_sector && guard++ < 1_000_000)
        {
            int pos = (int)cursor * mini_sector_size;
            if (pos + mini_sector_size <= mini_stream.Length)
                ms.Write(mini_stream, pos, mini_sector_size);
            else if (pos < mini_stream.Length)
                ms.Write(mini_stream, pos, mini_stream.Length - pos);

            if (ms.Length >= length)
                break;
            cursor = minifat[(int)cursor];
        }

        byte[] result = ms.ToArray();
        if (length < result.Length)
            Array.Resize(ref result, length);
        return result;
    }

    private static List<List<string>> ParseBiff8Workbook(byte[] stream)
    {
        var sst = new List<string>();
        var custom_formats = new Dictionary<int, string>();
        var xf_format_ids = new List<int>();
        bool is_1904 = false;

        var records = ReadBiffRecords(stream);
        for (int index = 0; index < records.Count; index++)
        {
            (ushort id, byte[] payload) = records[index];
            if (id == 0x0022 && payload.Length >= 2)
                is_1904 = BitConverter.ToUInt16(payload, 0) != 0;
            else if (id == 0x041E && payload.Length >= 5)
            {
                int fmt_id = BitConverter.ToUInt16(payload, 0);
                int pos = 2;
                custom_formats[fmt_id] = ReadBiff8UnicodeString16(payload, ref pos);
            }
            else if (id == 0x00E0 && payload.Length >= 4)
                xf_format_ids.Add(BitConverter.ToUInt16(payload, 2));
            else if (id == 0x00FC)
                sst = ParseSstWithContinues(records, index);
        }

        var cells = new Dictionary<(int row, int col), string>();
        int max_row = -1;
        int max_col = -1;
        int bof_depth = 0;
        bool in_first_sheet = false;
        bool first_sheet_done = false;
        (int row, int col) pending_formula_cell = (-1, -1);

        foreach ((ushort id, byte[] payload) in records)
        {
            if (id == 0x0809)
            {
                ushort dt = payload.Length >= 4 ? BitConverter.ToUInt16(payload, 2) : (ushort)0;
                bof_depth++;
                if (!first_sheet_done && bof_depth == 1 && dt == 0x0010)
                    in_first_sheet = true;
                continue;
            }
            if (id == 0x000A)
            {
                if (in_first_sheet && bof_depth == 1)
                {
                    in_first_sheet = false;
                    first_sheet_done = true;
                }
                if (bof_depth > 0)
                    bof_depth--;
                continue;
            }
            if (!in_first_sheet)
                continue;

            if (id == 0x00FD && payload.Length >= 10) // LABELSST
            {
                int r = BitConverter.ToUInt16(payload, 0);
                int c = BitConverter.ToUInt16(payload, 2);
                int sst_index = BitConverter.ToInt32(payload, 6);
                SetCell(cells, r, c, sst_index >= 0 && sst_index < sst.Count ? sst[sst_index] : string.Empty, ref max_row, ref max_col);
            }
            else if (id == 0x0204 && payload.Length >= 9) // LABEL
            {
                int r = BitConverter.ToUInt16(payload, 0);
                int c = BitConverter.ToUInt16(payload, 2);
                int pos = 6;
                string val = ReadBiff8UnicodeString16(payload, ref pos);
                SetCell(cells, r, c, val, ref max_row, ref max_col);
            }
            else if (id == 0x0203 && payload.Length >= 14) // NUMBER
            {
                int r = BitConverter.ToUInt16(payload, 0);
                int c = BitConverter.ToUInt16(payload, 2);
                int xf = BitConverter.ToUInt16(payload, 4);
                double num = BitConverter.ToDouble(payload, 6);
                SetCell(cells, r, c, FormatNumericCell(num, xf, xf_format_ids, custom_formats, is_1904), ref max_row, ref max_col);
            }
            else if (id == 0x027E && payload.Length >= 10) // RK
            {
                int r = BitConverter.ToUInt16(payload, 0);
                int c = BitConverter.ToUInt16(payload, 2);
                int xf = BitConverter.ToUInt16(payload, 4);
                double num = DecodeRk(BitConverter.ToUInt32(payload, 6));
                SetCell(cells, r, c, FormatNumericCell(num, xf, xf_format_ids, custom_formats, is_1904), ref max_row, ref max_col);
            }
            else if (id == 0x00BD && payload.Length >= 12) // MULRK
            {
                int r = BitConverter.ToUInt16(payload, 0);
                int col_first = BitConverter.ToUInt16(payload, 2);
                int rk_count = (payload.Length - 6) / 6;
                for (int i = 0; i < rk_count; i++)
                {
                    int off = 4 + i * 6;
                    int xf = BitConverter.ToUInt16(payload, off);
                    double num = DecodeRk(BitConverter.ToUInt32(payload, off + 2));
                    SetCell(cells, r, col_first + i, FormatNumericCell(num, xf, xf_format_ids, custom_formats, is_1904), ref max_row, ref max_col);
                }
            }
            else if (id == 0x0006 && payload.Length >= 14) // FORMULA
            {
                int r = BitConverter.ToUInt16(payload, 0);
                int c = BitConverter.ToUInt16(payload, 2);
                int xf = BitConverter.ToUInt16(payload, 4);
                if (payload[12] == 0xFF && payload[13] == 0xFF)
                    pending_formula_cell = payload[6] == 0 ? (r, c) : (-1, -1);
                else
                {
                    double num = BitConverter.ToDouble(payload, 6);
                    SetCell(cells, r, c, FormatNumericCell(num, xf, xf_format_ids, custom_formats, is_1904), ref max_row, ref max_col);
                }
            }
            else if (id == 0x0207 && pending_formula_cell.row >= 0) // STRING cached formula result
            {
                int pos = 0;
                string val = ReadBiff8UnicodeString16(payload, ref pos);
                SetCell(cells, pending_formula_cell.row, pending_formula_cell.col, val, ref max_row, ref max_col);
                pending_formula_cell = (-1, -1);
            }
        }

        if (max_row < 0)
            return [];

        var rows = new List<List<string>>(max_row + 1);
        for (int r = 0; r <= max_row; r++)
        {
            var row = new List<string>(max_col + 1);
            for (int c = 0; c <= max_col; c++)
                row.Add(cells.TryGetValue((r, c), out string? val) ? val : string.Empty);
            rows.Add(row);
        }
        return rows;
    }

    private static List<(ushort id, byte[] payload)> ReadBiffRecords(byte[] stream)
    {
        var list = new List<(ushort, byte[])>();
        int pos = 0;
        while (pos + 4 <= stream.Length)
        {
            ushort id = BitConverter.ToUInt16(stream, pos);
            ushort len = BitConverter.ToUInt16(stream, pos + 2);
            pos += 4;
            if (id == 0 && len == 0 && pos > 16)
                break;
            int take = Math.Min(len, stream.Length - pos);
            byte[] payload = new byte[take];
            Buffer.BlockCopy(stream, pos, payload, 0, take);
            list.Add((id, payload));
            pos += take;
        }
        return list;
    }

    private static List<string> ParseSstWithContinues(List<(ushort id, byte[] payload)> records, int sst_index)
    {
        var slices = new List<byte[]> { records[sst_index].payload };
        for (int i = sst_index + 1; i < records.Count && records[i].id == 0x003C; i++)
            slices.Add(records[i].payload);

        int slice_idx = 0;
        int offset = 4;
        if (slices[0].Length < 8)
            return [];

        int unique_count = BitConverter.ToInt32(slices[0], 4);
        offset = 8;
        var result = new List<string>(unique_count);

        byte ReadByte()
        {
            while (offset >= slices[slice_idx].Length)
            {
                slice_idx++;
                offset = 0;
            }
            return slices[slice_idx][offset++];
        }

        ushort ReadUInt16() => (ushort)(ReadByte() | (ReadByte() << 8));
        uint ReadUInt32() => (uint)(ReadByte() | (ReadByte() << 8) | (ReadByte() << 16) | (ReadByte() << 24));

        for (int s = 0; s < unique_count && slice_idx < slices.Count; s++)
        {
            if (slice_idx == slices.Count - 1 && offset + 3 > slices[slice_idx].Length)
                break;

            int char_count = ReadUInt16();
            byte flags = ReadByte();
            bool is_utf16 = (flags & 0x01) != 0;
            bool has_ext = (flags & 0x04) != 0;
            bool has_rich = (flags & 0x08) != 0;
            int runs = has_rich ? ReadUInt16() : 0;
            int ext_len = has_ext ? (int)ReadUInt32() : 0;

            var sb = new StringBuilder(char_count);
            int remaining = char_count;
            while (remaining > 0)
            {
                if (offset >= slices[slice_idx].Length)
                {
                    slice_idx++;
                    offset = 0;
                    if (slice_idx >= slices.Count)
                        break;
                    byte cont_flags = ReadByte();
                    is_utf16 = (cont_flags & 0x01) != 0;
                }

                int bytes_per_char = is_utf16 ? 2 : 1;
                int avail_chars = (slices[slice_idx].Length - offset) / bytes_per_char;
                int take_chars = Math.Min(remaining, avail_chars);
                if (take_chars <= 0)
                    break;

                if (is_utf16)
                    sb.Append(Encoding.Unicode.GetString(slices[slice_idx], offset, take_chars * 2));
                else
                    sb.Append(Encoding.Latin1.GetString(slices[slice_idx], offset, take_chars));

                offset += take_chars * bytes_per_char;
                remaining -= take_chars;
            }

            int skip_bytes = runs * 4 + ext_len;
            while (skip_bytes > 0 && slice_idx < slices.Count)
            {
                int step = Math.Min(skip_bytes, slices[slice_idx].Length - offset);
                offset += step;
                skip_bytes -= step;
                if (skip_bytes > 0)
                {
                    slice_idx++;
                    offset = 0;
                }
            }

            result.Add(sb.ToString());
        }

        return result;
    }

    private static string ReadBiff8UnicodeString16(byte[] buffer, ref int pos)
    {
        if (pos + 3 > buffer.Length)
            return string.Empty;

        int char_count = BitConverter.ToUInt16(buffer, pos);
        byte flags = buffer[pos + 2];
        pos += 3;
        bool is_utf16 = (flags & 0x01) != 0;
        if ((flags & 0x08) != 0) pos += 2;
        if ((flags & 0x04) != 0) pos += 4;

        int byte_count = char_count * (is_utf16 ? 2 : 1);
        int safe_bytes = Math.Min(byte_count, Math.Max(0, buffer.Length - pos));
        string text = is_utf16
            ? Encoding.Unicode.GetString(buffer, pos, safe_bytes)
            : Encoding.Latin1.GetString(buffer, pos, safe_bytes);
        pos += safe_bytes;
        return text;
    }

    private static double DecodeRk(uint rk)
    {
        bool div100 = (rk & 0x01) != 0;
        bool is_int = (rk & 0x02) != 0;
        double value = is_int
            ? ((int)(rk & 0xFFFFFFFC)) >> 2
            : BitConverter.Int64BitsToDouble(((long)(rk & 0xFFFFFFFC)) << 32);
        return div100 ? value / 100.0 : value;
    }

    private static string FormatNumericCell(double number, int xf_index, List<int> xf_format_ids,
        Dictionary<int, string> custom_formats, bool is_1904)
    {
        int fmt_id = xf_index >= 0 && xf_index < xf_format_ids.Count ? xf_format_ids[xf_index] : 0;
        if (IsDateFormat(fmt_id, custom_formats) && number >= 1 && number < 2958466)
        {
            DateTime dt = is_1904
                ? new DateTime(1904, 1, 1).AddDays(number)
                : DateTime.FromOADate(number);
            return dt.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
        }

        decimal dec = Math.Round((decimal)number, 2);
        return dec.ToString("0.##", CultureInfo.InvariantCulture);
    }

    private static bool IsDateFormat(int fmt_id, Dictionary<int, string> custom_formats)
    {
        if (fmt_id is (>= 14 and <= 22) or (>= 45 and <= 47))
            return true;

        if (custom_formats.TryGetValue(fmt_id, out string? fmt))
            return fmt.Contains('y', StringComparison.OrdinalIgnoreCase) && fmt.Contains('d', StringComparison.OrdinalIgnoreCase);

        return false;
    }

    private static void SetCell(Dictionary<(int row, int col), string> cells, int row, int col, string value, ref int max_row, ref int max_col)
    {
        cells[(row, col)] = value;
        if (row > max_row) max_row = row;
        if (col > max_col) max_col = col;
    }

    private static List<List<string>> ParseSpreadsheetMlXml(string xml_text)
    {
        var doc = XDocument.Parse(xml_text);
        XNamespace ss = "urn:schemas-microsoft-com:office:spreadsheet";
        XElement? worksheet = doc.Root?.Element(ss + "Worksheet") ?? doc.Root?.Elements().FirstOrDefault(e => e.Name.LocalName == "Worksheet");
        XElement? table = worksheet?.Element(ss + "Table") ?? worksheet?.Elements().FirstOrDefault(e => e.Name.LocalName == "Table");
        if (table is null)
            return [];

        var result = new List<List<string>>();
        foreach (XElement row_el in table.Elements().Where(e => e.Name.LocalName == "Row"))
        {
            var row = new List<string>();
            int col_cursor = 1;
            foreach (XElement cell_el in row_el.Elements().Where(e => e.Name.LocalName == "Cell"))
            {
                string? index_attr = cell_el.Attributes().FirstOrDefault(a => a.Name.LocalName == "Index")?.Value;
                if (int.TryParse(index_attr, out int explicit_index) && explicit_index > col_cursor)
                {
                    while (col_cursor < explicit_index)
                    {
                        row.Add(string.Empty);
                        col_cursor++;
                    }
                }
                XElement? data_el = cell_el.Elements().FirstOrDefault(e => e.Name.LocalName == "Data");
                row.Add(data_el?.Value ?? string.Empty);
                col_cursor++;
            }
            result.Add(row);
        }
        return result;
    }

    private static List<List<string>> ParseHtmlTable(string html_text)
    {
        var result = new List<List<string>>();
        foreach (Match tr in TrRegex().Matches(html_text))
        {
            var row = new List<string>();
            foreach (Match td in TdRegex().Matches(tr.Groups[1].Value))
            {
                string clean = TagRegex().Replace(td.Groups[1].Value, string.Empty);
                row.Add(WebUtility.HtmlDecode(clean).Trim());
            }
            if (row.Count > 0)
                result.Add(row);
        }
        return result;
    }

    private static List<List<string>> ParseDelimitedText(string text)
    {
        var result = new List<List<string>>();
        using var reader = new StringReader(text);
        char? separator = null;
        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;
            separator ??= line.Contains('\t') ? '\t' : ',';
            result.Add(SplitCsvLine(line, separator.Value));
        }
        return result;
    }

    private static List<string> SplitCsvLine(string line, char separator)
    {
        var fields = new List<string>();
        var current = new StringBuilder();
        bool in_quotes = false;
        for (int i = 0; i < line.Length; i++)
        {
            char ch = line[i];
            if (ch == '"')
            {
                if (in_quotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else
                    in_quotes = !in_quotes;
            }
            else if (ch == separator && !in_quotes)
            {
                fields.Add(current.ToString());
                current.Clear();
            }
            else
                current.Append(ch);
        }
        fields.Add(current.ToString());
        return fields;
    }

    private static string DecodeTextWithBom(byte[] bytes)
    {
        if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE)
            return Encoding.Unicode.GetString(bytes, 2, bytes.Length - 2);
        if (bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF)
            return Encoding.BigEndianUnicode.GetString(bytes, 2, bytes.Length - 2);
        if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
            return Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);
        return Encoding.UTF8.GetString(bytes);
    }

    [GeneratedRegex(@"<tr[^>]*>(.*?)</tr>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex TrRegex();

    [GeneratedRegex(@"<t[hd][^>]*>(.*?)</t[hd]>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex TdRegex();

    [GeneratedRegex(@"<[^>]+>")]
    private static partial Regex TagRegex();
}

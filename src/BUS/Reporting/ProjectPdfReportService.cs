using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;
using DTO.Statistics;

namespace BUS.Reporting;

/// <summary>
/// Xuất báo cáo thống kê dự án ra tệp PDF 1.4 nhúng font TrueType Unicode (CIDFontType2 + ToUnicode)
/// giúp chữ tiếng Việt có dấu hiển thị rõ nét, đọc và tìm kiếm được trên mọi trình xem PDF [D7].
/// </summary>
public sealed class ProjectPdfReportService
{
    private const float PageWidth = 595f;
    private const float PageHeight = 842f;
    private const float MarginX = 42f;
    private const float TopY = 795f;
    private const float BottomMarginY = 50f;

    public void ExportSummaryToPdf(string file_path, ProjectStatisticsSummaryDto summary)
    {
        if (string.IsNullOrWhiteSpace(file_path))
            throw new ArgumentException("Đường dẫn tệp PDF không được để trống.");
        ArgumentNullException.ThrowIfNull(summary);

        (string regular_path, string bold_path) = ResolveUnicodeFonts();
        var regular_font = TrueTypeFontInfo.Load(regular_path);
        var bold_font = TrueTypeFontInfo.Load(bold_path);

        var writer = new PdfDocumentBuilder(regular_font, bold_font, PageWidth, PageHeight, MarginX, TopY, BottomMarginY);

        // Phần đầu báo cáo
        writer.DrawCenteredText("BÁO CÁO THỐNG KÊ DỰ ÁN THEO KỲ", 15f, use_bold: true, space_after: 18f);
        writer.DrawText($"Kỳ lọc: {summary.PeriodTitle}", 10.5f, use_bold: true, space_after: 15f);
        writer.DrawText($"Tiêu chí nhóm ngân sách: {summary.BudgetCriterionTitle}   |   Ngày lập: {summary.GeneratedAt:dd/MM/yyyy HH:mm}", 9.5f, use_bold: false, space_after: 20f);

        // Bảng 1: Thống kê tổng ngân sách dự án
        writer.DrawSectionTable(
            section_title: $"1. Thống kê tổng ngân sách dự án (nhóm theo {summary.BudgetCriterionTitle})",
            name_header: summary.BudgetCriterionTitle,
            count_header: "Số dự án",
            amount_header: "Tổng ngân sách (VNĐ)",
            rows: summary.BudgetRows,
            total_amount: summary.TotalBudget);

        // Bảng 2: Thống kê phụ cấp dự án
        writer.DrawSectionTable(
            section_title: "2. Thống kê phụ cấp dự án (tổng hợp từ Chi tiết dự án)",
            name_header: "Dự án",
            count_header: "Số chi tiết",
            amount_header: "Tổng phụ cấp (VNĐ)",
            rows: summary.AllowanceRows,
            total_amount: summary.TotalAllowance);

        // Bảng 3: Thống kê tiền thưởng dự án
        writer.DrawSectionTable(
            section_title: "3. Thống kê tiền thưởng dự án (tổng hợp từ Chi tiết dự án)",
            name_header: "Dự án",
            count_header: "Số chi tiết",
            amount_header: "Tổng thưởng (VNĐ)",
            rows: summary.BonusRows,
            total_amount: summary.TotalBonus);

        string? parent_dir = Path.GetDirectoryName(file_path);
        if (!string.IsNullOrWhiteSpace(parent_dir))
            Directory.CreateDirectory(parent_dir);

        File.WriteAllBytes(file_path, writer.BuildPdfBytes());
    }

    private static (string Regular, string Bold) ResolveUnicodeFonts()
    {
        string fonts_dir = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);
        if (string.IsNullOrWhiteSpace(fonts_dir))
            fonts_dir = @"C:\Windows\Fonts";

        (string Regular, string Bold)[] candidates =
        [
            ("tahoma.ttf", "tahomabd.ttf"),
            ("arial.ttf", "arialbd.ttf"),
            ("segoeui.ttf", "segoeuib.ttf"),
            ("times.ttf", "timesbd.ttf")
        ];

        foreach ((string reg_name, string bold_name) in candidates)
        {
            string reg_path = Path.Combine(fonts_dir, reg_name);
            if (!File.Exists(reg_path))
                continue;

            string bold_path = Path.Combine(fonts_dir, bold_name);
            return (reg_path, File.Exists(bold_path) ? bold_path : reg_path);
        }

        throw new InvalidOperationException("Không tìm thấy font TrueType hỗ trợ tiếng Việt (Tahoma/Arial/Segoe UI) trong thư mục Font của Windows.");
    }

    private sealed class PdfDocumentBuilder
    {
        private readonly TrueTypeFontInfo _regular;
        private readonly TrueTypeFontInfo _bold;
        private readonly float _page_width;
        private readonly float _page_height;
        private readonly float _margin_x;
        private readonly float _top_y;
        private readonly float _bottom_y;
        private readonly float _content_width;
        private readonly List<StringBuilder> _pages = [];
        private float _cursor_y;

        public PdfDocumentBuilder(
            TrueTypeFontInfo regular,
            TrueTypeFontInfo bold,
            float page_width,
            float page_height,
            float margin_x,
            float top_y,
            float bottom_y)
        {
            _regular = regular;
            _bold = bold;
            _page_width = page_width;
            _page_height = page_height;
            _margin_x = margin_x;
            _top_y = top_y;
            _bottom_y = bottom_y;
            _content_width = page_width - (margin_x * 2);
            AddNewPage();
        }

        private StringBuilder CurrentStream => _pages[^1];

        private void AddNewPage()
        {
            _pages.Add(new StringBuilder());
            _cursor_y = _top_y;
        }

        private void EnsureSpace(float needed_height)
        {
            if (_cursor_y - needed_height < _bottom_y)
                AddNewPage();
        }

        public void DrawCenteredText(string text, float font_size, bool use_bold, float space_after)
        {
            EnsureSpace(space_after + 6f);
            TrueTypeFontInfo font = use_bold ? _bold : _regular;
            float width = font.MeasureWidth(text, font_size);
            float x = Math.Max(_margin_x, (_page_width - width) / 2f);
            EmitText(x, _cursor_y, text, font_size, use_bold);
            _cursor_y -= space_after;
        }

        public void DrawText(string text, float font_size, bool use_bold, float space_after)
        {
            EnsureSpace(space_after + 6f);
            EmitText(_margin_x, _cursor_y, text, font_size, use_bold);
            _cursor_y -= space_after;
        }

        public void DrawSectionTable(
            string section_title,
            string name_header,
            string count_header,
            string amount_header,
            IReadOnlyList<ProjectStatisticsRowDto> rows,
            decimal total_amount)
        {
            const float row_height = 20f;
            float[] col_widths = [42f, 240f, 100f, _content_width - 382f];

            EnsureSpace(row_height * 3 + 22f);
            EmitText(_margin_x, _cursor_y, section_title, 11f, use_bold: true);
            _cursor_y -= 14f;

            DrawTableHeader(col_widths, ["STT", name_header, count_header, amount_header], row_height);

            if (rows.Count == 0)
            {
                EnsureSpace(row_height);
                DrawTableRow(col_widths, ["-", "Không có dữ liệu trong kỳ lọc này", "0", "0"], row_height, bold_row: false, fill_gray: false);
            }
            else
            {
                for (int i = 0; i < rows.Count; i++)
                {
                    if (_cursor_y - row_height < _bottom_y)
                    {
                        AddNewPage();
                        DrawTableHeader(col_widths, ["STT", name_header, count_header, amount_header], row_height);
                    }

                    ProjectStatisticsRowDto row = rows[i];
                    DrawTableRow(
                        col_widths,
                        [
                            (i + 1).ToString(CultureInfo.InvariantCulture),
                            row.GroupName,
                            row.ItemCount.ToString("N0", CultureInfo.InvariantCulture),
                            row.Amount.ToString("N0", CultureInfo.InvariantCulture)
                        ],
                        row_height,
                        bold_row: false,
                        fill_gray: false);
                }
            }

            EnsureSpace(row_height + 10f);
            int total_count = 0;
            foreach (ProjectStatisticsRowDto row in rows)
                total_count += row.ItemCount;

            DrawTableRow(
                col_widths,
                ["", "TỔNG CỘNG", total_count.ToString("N0", CultureInfo.InvariantCulture), total_amount.ToString("N0", CultureInfo.InvariantCulture)],
                row_height,
                bold_row: true,
                fill_gray: true);

            _cursor_y -= 16f;
        }

        private void DrawTableHeader(float[] col_widths, string[] headers, float row_height)
        {
            DrawTableRow(col_widths, headers, row_height, bold_row: true, fill_gray: true);
        }

        private void DrawTableRow(float[] col_widths, string[] cells, float row_height, bool bold_row, bool fill_gray)
        {
            float bottom_y = _cursor_y - row_height;
            float x = _margin_x;

            if (fill_gray)
            {
                CurrentStream.AppendFormat(
                    CultureInfo.InvariantCulture,
                    "0.93 0.95 0.97 rg {0:0.##} {1:0.##} {2:0.##} {3:0.##} re f 0 0 0 rg\n",
                    _margin_x, bottom_y, _content_width, row_height);
            }

            CurrentStream.Append("0.55 w 0.35 0.38 0.42 RG ");
            for (int c = 0; c < col_widths.Length; c++)
            {
                float w = col_widths[c];
                CurrentStream.AppendFormat(
                    CultureInfo.InvariantCulture,
                    "{0:0.##} {1:0.##} {2:0.##} {3:0.##} re S\n",
                    x, bottom_y, w, row_height);

                TrueTypeFontInfo font = bold_row ? _bold : _regular;
                string clipped = ClipTextToWidth(cells[c], font, 9f, w - 8f);
                float text_y = bottom_y + 6.5f;

                if (c >= 2)
                {
                    float tw = font.MeasureWidth(clipped, 9f);
                    EmitText(x + w - 4f - tw, text_y, clipped, 9f, bold_row);
                }
                else
                {
                    EmitText(x + 4f, text_y, clipped, 9f, bold_row);
                }

                x += w;
            }
            CurrentStream.Append("0 0 0 RG\n");
            _cursor_y = bottom_y;
        }

        private static string ClipTextToWidth(string text, TrueTypeFontInfo font, float font_size, float max_width)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            if (font.MeasureWidth(text, font_size) <= max_width)
                return text;

            string ellipsis = "...";
            for (int len = text.Length - 1; len > 0; len--)
            {
                string candidate = text[..len] + ellipsis;
                if (font.MeasureWidth(candidate, font_size) <= max_width)
                    return candidate;
            }
            return ellipsis;
        }

        private void EmitText(float x, float y, string text, float font_size, bool use_bold)
        {
            TrueTypeFontInfo font = use_bold ? _bold : _regular;
            string font_alias = use_bold ? "F2" : "F1";
            string hex_gids = font.EncodeTextAsHexGlyphIds(text);

            CurrentStream.AppendFormat(
                CultureInfo.InvariantCulture,
                "BT /{0} {1:0.##} Tf {2:0.##} {3:0.##} Td <{4}> Tj ET\n",
                font_alias, font_size, x, y, hex_gids);
        }

        public byte[] BuildPdfBytes()
        {
            // Bố trí số đối tượng PDF cố định để mọi tham chiếu chéo chính xác:
            // 1: Catalog, 2: Pages
            // 3..7: Regular Font (/F1) [Type0, CIDFont, FontDescriptor, FontFile2, ToUnicode]
            // 8..12: Bold Font (/F2) [Type0, CIDFont, FontDescriptor, FontFile2, ToUnicode]
            // Từ 13 trở đi: mỗi trang gồm [Page obj, Content stream obj]
            var objects = new Dictionary<int, byte[]>();

            AddFontObjects(objects, _regular, type0_id: 3, cid_id: 4, desc_id: 5, stream_id: 6, to_unicode_id: 7);
            AddFontObjects(objects, _bold, type0_id: 8, cid_id: 9, desc_id: 10, stream_id: 11, to_unicode_id: 12);

            var page_ids = new List<int>(_pages.Count);
            int next_id = 13;
            for (int i = 0; i < _pages.Count; i++)
            {
                int page_id = next_id++;
                int content_id = next_id++;
                page_ids.Add(page_id);

                // Thêm số trang ở chân trang
                string footer = $"Trang {i + 1}/{_pages.Count}";
                float fw = _regular.MeasureWidth(footer, 8.5f);
                _pages[i].AppendFormat(
                    CultureInfo.InvariantCulture,
                    "BT /F1 8.5 Tf {0:0.##} 24 Td <{1}> Tj ET\n",
                    (_page_width - fw) / 2f,
                    _regular.EncodeTextAsHexGlyphIds(footer));

                byte[] compressed_content = CompressZlib(Encoding.ASCII.GetBytes(_pages[i].ToString()));
                objects[content_id] = BuildStreamObject($"<< /Length {compressed_content.Length} /Filter /FlateDecode >>", compressed_content);

                objects[page_id] = Encoding.ASCII.GetBytes(
                    $"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 {_page_width.ToString(CultureInfo.InvariantCulture)} {_page_height.ToString(CultureInfo.InvariantCulture)}] " +
                    $"/Resources << /Font << /F1 3 0 R /F2 8 0 R >> >> /Contents {content_id} 0 R >>");
            }

            string kids = string.Join(" ", page_ids.ConvertAll(id => $"{id} 0 R"));
            objects[1] = Encoding.ASCII.GetBytes("<< /Type /Catalog /Pages 2 0 R >>");
            objects[2] = Encoding.ASCII.GetBytes($"<< /Type /Pages /Count {page_ids.Count} /Kids [{kids}] >>");

            using var ms = new MemoryStream();
            void WriteAscii(string s)
            {
                byte[] b = Encoding.ASCII.GetBytes(s);
                ms.Write(b, 0, b.Length);
            }

            WriteAscii("%PDF-1.4\n%\xE2\xE3\xCF\xD3\n");
            int total_objects = next_id - 1;
            long[] offsets = new long[total_objects + 1];

            for (int id = 1; id <= total_objects; id++)
            {
                offsets[id] = ms.Position;
                WriteAscii($"{id} 0 obj\n");
                byte[] body = objects[id];
                ms.Write(body, 0, body.Length);
                WriteAscii("\nendobj\n");
            }

            long xref_offset = ms.Position;
            WriteAscii($"xref\n0 {total_objects + 1}\n");
            WriteAscii("0000000000 65535 f \n");
            for (int id = 1; id <= total_objects; id++)
                WriteAscii($"{offsets[id]:D10} 00000 n \n");

            WriteAscii($"trailer\n<< /Size {total_objects + 1} /Root 1 0 R >>\nstartxref\n{xref_offset}\n%%EOF\n");
            return ms.ToArray();
        }

        private static void AddFontObjects(
            Dictionary<int, byte[]> objects,
            TrueTypeFontInfo font,
            int type0_id,
            int cid_id,
            int desc_id,
            int stream_id,
            int to_unicode_id)
        {
            byte[] compressed_font = CompressZlib(font.RawFontBytes);
            objects[stream_id] = BuildStreamObject(
                $"<< /Length {compressed_font.Length} /Length1 {font.RawFontBytes.Length} /Filter /FlateDecode >>",
                compressed_font);

            string base_font_name = font.PdfFontName;
            objects[desc_id] = Encoding.ASCII.GetBytes(
                $"<< /Type /FontDescriptor /FontName /{base_font_name} /Flags 32 " +
                $"/FontBBox [-500 -300 1200 1000] /ItalicAngle 0 " +
                $"/Ascent {font.Ascent} /Descent {font.Descent} /CapHeight {font.Ascent} /StemV 80 " +
                $"/FontFile2 {stream_id} 0 R >>");

            string widths = font.BuildWidthsArray();
            objects[cid_id] = Encoding.ASCII.GetBytes(
                $"<< /Type /Font /Subtype /CIDFontType2 /BaseFont /{base_font_name} " +
                $"/CIDSystemInfo << /Registry (Adobe) /Ordering (Identity) /Supplement 0 >> " +
                $"/FontDescriptor {desc_id} 0 R /DW 600 /W [{widths}] /CIDToGIDMap /Identity >>");

            byte[] cmap_bytes = CompressZlib(Encoding.ASCII.GetBytes(font.BuildToUnicodeCMap()));
            objects[to_unicode_id] = BuildStreamObject($"<< /Length {cmap_bytes.Length} /Filter /FlateDecode >>", cmap_bytes);

            objects[type0_id] = Encoding.ASCII.GetBytes(
                $"<< /Type /Font /Subtype /Type0 /BaseFont /{base_font_name} " +
                $"/Encoding /Identity-H /DescendantFonts [{cid_id} 0 R] /ToUnicode {to_unicode_id} 0 R >>");
        }

        private static byte[] BuildStreamObject(string dict_header, byte[] payload)
        {
            using var ms = new MemoryStream();
            byte[] head = Encoding.ASCII.GetBytes(dict_header + "\nstream\n");
            byte[] tail = Encoding.ASCII.GetBytes("\nendstream");
            ms.Write(head, 0, head.Length);
            ms.Write(payload, 0, payload.Length);
            ms.Write(tail, 0, tail.Length);
            return ms.ToArray();
        }

        private static byte[] CompressZlib(byte[] data)
        {
            using var output = new MemoryStream();
            using (var zlib = new ZLibStream(output, CompressionLevel.Optimal, leaveOpen: true))
                zlib.Write(data, 0, data.Length);
            return output.ToArray();
        }
    }

    private sealed class TrueTypeFontInfo
    {
        public required byte[] RawFontBytes { get; init; }
        public required string PdfFontName { get; init; }
        public required int UnitsPerEm { get; init; }
        public required int Ascent { get; init; }
        public required int Descent { get; init; }
        public required ushort[] AdvanceWidths { get; init; }
        public required Dictionary<char, ushort> CharToGlyph { get; init; }

        private readonly Dictionary<ushort, char> _used_glyph_unicode = new() { [0] = '\0' };

        public static TrueTypeFontInfo Load(string font_path)
        {
            byte[] data = File.ReadAllBytes(font_path);
            var tables = ReadTableOffsets(data);

            int head = tables["head"];
            int units_per_em = ReadUInt16(data, head + 18);

            int hhea = tables["hhea"];
            int ascent = ScaleMetric(ReadInt16(data, hhea + 4), units_per_em);
            int descent = ScaleMetric(ReadInt16(data, hhea + 6), units_per_em);
            int num_hmetrics = ReadUInt16(data, hhea + 34);

            int maxp = tables["maxp"];
            int num_glyphs = ReadUInt16(data, maxp + 4);

            int hmtx = tables["hmtx"];
            ushort[] advance_widths = new ushort[num_glyphs];
            ushort last_aw = 500;
            for (int g = 0; g < num_glyphs; g++)
            {
                if (g < num_hmetrics)
                {
                    last_aw = ReadUInt16(data, hmtx + g * 4);
                    advance_widths[g] = last_aw;
                }
                else
                {
                    advance_widths[g] = last_aw;
                }
            }

            Dictionary<char, ushort> char_to_glyph = ParseUnicodeCmap(data, tables["cmap"]);
            string raw_name = Path.GetFileNameWithoutExtension(font_path);
            var safe_name = new StringBuilder("VietFont_");
            foreach (char ch in raw_name)
                safe_name.Append(char.IsLetterOrDigit(ch) ? ch : '_');

            return new TrueTypeFontInfo
            {
                RawFontBytes = data,
                PdfFontName = safe_name.ToString(),
                UnitsPerEm = units_per_em,
                Ascent = ascent,
                Descent = descent,
                AdvanceWidths = advance_widths,
                CharToGlyph = char_to_glyph
            };
        }

        public float MeasureWidth(string text, float font_size)
        {
            float total = 0f;
            foreach (char ch in text)
            {
                ushort gid = ResolveGlyph(ch);
                float unit_w = gid < AdvanceWidths.Length
                    ? AdvanceWidths[gid] * 1000f / UnitsPerEm
                    : 500f;
                total += unit_w * font_size / 1000f;
            }
            return total;
        }

        public string EncodeTextAsHexGlyphIds(string text)
        {
            var sb = new StringBuilder(text.Length * 4);
            foreach (char ch in text)
            {
                ushort gid = ResolveGlyph(ch);
                _used_glyph_unicode.TryAdd(gid, ch);
                sb.AppendFormat(CultureInfo.InvariantCulture, "{0:X4}", gid);
            }
            return sb.ToString();
        }

        public string BuildWidthsArray()
        {
            var sb = new StringBuilder();
            var sorted_gids = new List<ushort>(_used_glyph_unicode.Keys);
            sorted_gids.Sort();
            foreach (ushort gid in sorted_gids)
            {
                int w = gid < AdvanceWidths.Length
                    ? ScaleMetric(AdvanceWidths[gid], UnitsPerEm)
                    : 500;
                sb.AppendFormat(CultureInfo.InvariantCulture, "{0} [{1}] ", gid, w);
            }
            return sb.ToString();
        }

        public string BuildToUnicodeCMap()
        {
            var pairs = new List<(ushort Gid, char Unicode)>();
            foreach (KeyValuePair<ushort, char> kvp in _used_glyph_unicode)
            {
                if (kvp.Key != 0 && kvp.Value != '\0')
                    pairs.Add((kvp.Key, kvp.Value));
            }
            pairs.Sort((a, b) => a.Gid.CompareTo(b.Gid));

            var sb = new StringBuilder();
            sb.AppendLine("/CIDInit /ProcSet findresource begin");
            sb.AppendLine("12 dict begin");
            sb.AppendLine("begincmap");
            sb.AppendLine("/CIDSystemInfo << /Registry (Adobe) /Ordering (UCS) /Supplement 0 >> def");
            sb.AppendLine("/CMapName /Adobe-Identity-UCS def");
            sb.AppendLine("/CMapType 2 def");
            sb.AppendLine("1 begincodespacerange");
            sb.AppendLine("<0000> <FFFF>");
            sb.AppendLine("endcodespacerange");

            // PDF quy định mỗi khối beginbfchar tối đa 100 mục.
            for (int start = 0; start < pairs.Count; start += 100)
            {
                int batch_count = Math.Min(100, pairs.Count - start);
                sb.AppendLine($"{batch_count} beginbfchar");
                for (int i = 0; i < batch_count; i++)
                {
                    (ushort gid, char ch) = pairs[start + i];
                    sb.AppendLine($"<{gid:X4}> <{(ushort)ch:X4}>");
                }
                sb.AppendLine("endbfchar");
            }

            sb.AppendLine("endcmap");
            sb.AppendLine("CMapName currentdict /CMap defineresource pop");
            sb.AppendLine("end");
            sb.AppendLine("end");
            return sb.ToString();
        }

        private ushort ResolveGlyph(char ch)
        {
            if (CharToGlyph.TryGetValue(ch, out ushort gid))
                return gid;
            if (CharToGlyph.TryGetValue('?', out ushort fallback))
                return fallback;
            return 0;
        }

        private static Dictionary<string, int> ReadTableOffsets(byte[] data)
        {
            int num_tables = ReadUInt16(data, 4);
            var map = new Dictionary<string, int>(num_tables, StringComparer.Ordinal);
            for (int i = 0; i < num_tables; i++)
            {
                int record_pos = 12 + i * 16;
                string tag = Encoding.ASCII.GetString(data, record_pos, 4);
                int offset = (int)ReadUInt32(data, record_pos + 8);
                map[tag] = offset;
            }
            return map;
        }

        private static Dictionary<char, ushort> ParseUnicodeCmap(byte[] data, int cmap_offset)
        {
            int num_subtables = ReadUInt16(data, cmap_offset + 2);
            int selected_format4_offset = -1;
            int fallback_format4_offset = -1;

            for (int i = 0; i < num_subtables; i++)
            {
                int pos = cmap_offset + 4 + i * 8;
                ushort platform_id = ReadUInt16(data, pos);
                ushort encoding_id = ReadUInt16(data, pos + 2);
                int subtable_offset = cmap_offset + (int)ReadUInt32(data, pos + 4);
                ushort format = ReadUInt16(data, subtable_offset);

                if (format == 4)
                {
                    if (platform_id == 3 && encoding_id == 1)
                        selected_format4_offset = subtable_offset;
                    else if (platform_id == 0 && fallback_format4_offset < 0)
                        fallback_format4_offset = subtable_offset;
                }
            }

            int table = selected_format4_offset >= 0 ? selected_format4_offset : fallback_format4_offset;
            if (table < 0)
                throw new InvalidOperationException("Font TrueType không có bảng cmap Format 4 Unicode.");

            int seg_count = ReadUInt16(data, table + 6) / 2;
            int end_code_pos = table + 14;
            int start_code_pos = end_code_pos + (seg_count * 2) + 2;
            int id_delta_pos = start_code_pos + (seg_count * 2);
            int id_range_offset_pos = id_delta_pos + (seg_count * 2);

            var map = new Dictionary<char, ushort>();
            for (int seg = 0; seg < seg_count; seg++)
            {
                ushort end_code = ReadUInt16(data, end_code_pos + seg * 2);
                ushort start_code = ReadUInt16(data, start_code_pos + seg * 2);
                short id_delta = ReadInt16(data, id_delta_pos + seg * 2);
                ushort id_range_offset = ReadUInt16(data, id_range_offset_pos + seg * 2);

                if (start_code == 0xFFFF && end_code == 0xFFFF)
                    break;

                for (int code = start_code; code <= end_code; code++)
                {
                    ushort glyph_id;
                    if (id_range_offset == 0)
                    {
                        glyph_id = unchecked((ushort)(code + id_delta));
                    }
                    else
                    {
                        int range_slot = id_range_offset_pos + seg * 2;
                        int glyph_addr = range_slot + id_range_offset + (code - start_code) * 2;
                        ushort raw_gid = ReadUInt16(data, glyph_addr);
                        glyph_id = raw_gid == 0 ? (ushort)0 : unchecked((ushort)(raw_gid + id_delta));
                    }

                    if (glyph_id != 0)
                        map[(char)code] = glyph_id;
                }
            }
            return map;
        }

        private static int ScaleMetric(int value, int units_per_em)
            => (int)Math.Round(value * 1000.0 / units_per_em);

        private static ushort ReadUInt16(byte[] data, int pos)
            => (ushort)((data[pos] << 8) | data[pos + 1]);

        private static short ReadInt16(byte[] data, int pos)
            => unchecked((short)((data[pos] << 8) | data[pos + 1]));

        private static uint ReadUInt32(byte[] data, int pos)
            => ((uint)data[pos] << 24) | ((uint)data[pos + 1] << 16) | ((uint)data[pos + 2] << 8) | data[pos + 3];
    }
}

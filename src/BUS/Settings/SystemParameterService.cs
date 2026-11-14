using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using DAO.Settings;
using DTO.Settings;

namespace BUS.Settings;

/// <summary>Nghiệp vụ quản lý tham số hệ thống: kiểm tra kiểu dữ liệu, miền giá trị và tìm kiếm trên ArrayList.</summary>
public sealed class SystemParameterService(ISystemParameterDao dao)
{
    private static readonly string[] _allowed_types = ["INT", "DECIMAL", "STRING"];

    public Task<List<SystemParameterDto>> GetAllAsync(CancellationToken cancellation_token = default)
        => dao.GetAllAsync(cancellation_token);

    public Task<SystemParameterDto?> GetByCodeAsync(string code, CancellationToken cancellation_token = default)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Mã tham số không được để trống.");
        return dao.GetByCodeAsync(code.Trim().ToUpperInvariant(), cancellation_token);
    }

    public async Task<string> AddAsync(SystemParameterDto item, CancellationToken cancellation_token = default)
    {
        ValidateAndNormalize(item);
        if (await dao.ExistsAsync(item.Code, cancellation_token))
            throw new InvalidOperationException($"Mã tham số '{item.Code}' đã tồn tại trong hệ thống.");

        await dao.InsertAsync(item, cancellation_token);
        return item.Code;
    }

    public async Task<string> UpdateAsync(SystemParameterDto item, CancellationToken cancellation_token = default)
    {
        ValidateAndNormalize(item);
        if (!await dao.ExistsAsync(item.Code, cancellation_token))
            throw new InvalidOperationException($"Không tìm thấy tham số '{item.Code}' để cập nhật.");

        await dao.UpdateAsync(item, cancellation_token);
        return item.Code;
    }

    public Task<string> SaveAsync(SystemParameterDto item, bool is_edit_mode = false, CancellationToken cancellation_token = default)
        => is_edit_mode ? UpdateAsync(item, cancellation_token) : AddAsync(item, cancellation_token);

    public Task DeleteAsync(string code, CancellationToken cancellation_token = default)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Vui lòng chọn mã tham số cần xóa.");
        return dao.DeleteAsync(code.Trim().ToUpperInvariant(), cancellation_token);
    }

    /// <summary>Đọc giá trị số thập phân của tham số từ CSDL để sử dụng tại các màn hình nghiệp vụ.</summary>
    public async Task<decimal> GetDecimalValueAsync(string code, decimal default_value, CancellationToken cancellation_token = default)
    {
        SystemParameterDto? parameter = await dao.GetByCodeAsync(code.Trim().ToUpperInvariant(), cancellation_token);
        if (parameter is null)
            return default_value;

        return TryParseDecimal(parameter.Value, out decimal parsed_value) ? parsed_value : default_value;
    }

    /// <summary>Đọc giá trị số nguyên của tham số từ CSDL (ví dụ: STANDARD_WORK_DAYS).</summary>
    public async Task<int> GetIntValueAsync(string code, int default_value, CancellationToken cancellation_token = default)
    {
        SystemParameterDto? parameter = await dao.GetByCodeAsync(code.Trim().ToUpperInvariant(), cancellation_token);
        if (parameter is null)
            return default_value;

        return int.TryParse(parameter.Value.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed_value)
            ? parsed_value
            : default_value;
    }

    public ArrayList Search(ArrayList source, string? term)
    {
        var result = new ArrayList();
        string keyword = term?.Trim() ?? string.Empty;
        foreach (object? obj in source)
        {
            if (obj is not SystemParameterDto item)
                continue;

            if (keyword.Length == 0
                || item.Code.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                || item.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                || item.Value.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                || item.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                result.Add(item);
            }
        }
        return result;
    }

    private static void ValidateAndNormalize(SystemParameterDto item)
    {
        ArgumentNullException.ThrowIfNull(item);

        item.Code = (item.Code?.Trim() ?? string.Empty).ToUpperInvariant();
        item.Name = item.Name?.Trim() ?? string.Empty;
        item.Value = item.Value?.Trim() ?? string.Empty;
        item.ValueType = (item.ValueType?.Trim() ?? "DECIMAL").ToUpperInvariant();
        item.Description = item.Description?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(item.Code) || item.Code.Length > 50)
            throw new ArgumentException("Mã tham số bắt buộc nhập và tối đa 50 ký tự.");
        if (string.IsNullOrWhiteSpace(item.Name) || item.Name.Length > 150)
            throw new ArgumentException("Tên tham số bắt buộc nhập và tối đa 150 ký tự.");
        if (string.IsNullOrWhiteSpace(item.Value) || item.Value.Length > 200)
            throw new ArgumentException("Giá trị tham số bắt buộc nhập và tối đa 200 ký tự.");
        if (!_allowed_types.Contains(item.ValueType))
            throw new ArgumentException("Kiểu dữ liệu chỉ hỗ trợ INT, DECIMAL hoặc STRING.");
        if (item.MinValue.HasValue && item.MaxValue.HasValue && item.MinValue.Value > item.MaxValue.Value)
            throw new ArgumentException("Giá trị tối thiểu không được lớn hơn giá trị tối đa.");
        if (item.Description.Length > 500)
            throw new ArgumentException("Mô tả tham số tối đa 500 ký tự.");

        // Kiểm tra kiểu dữ liệu và miền giá trị [MinValue, MaxValue] ngay ở BUS.
        if (item.ValueType == "INT")
        {
            if (!int.TryParse(item.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int int_value))
                throw new ArgumentException($"Tham số '{item.Code}' yêu cầu kiểu số nguyên (INT).");

            ValidateRange(item.Code, int_value, item.MinValue, item.MaxValue);
            item.Value = int_value.ToString(CultureInfo.InvariantCulture);
        }
        else if (item.ValueType == "DECIMAL")
        {
            if (!TryParseDecimal(item.Value, out decimal dec_value))
                throw new ArgumentException($"Tham số '{item.Code}' yêu cầu kiểu số (DECIMAL).");

            ValidateRange(item.Code, dec_value, item.MinValue, item.MaxValue);
            item.Value = dec_value.ToString("0.##", CultureInfo.InvariantCulture);
        }
    }

    private static void ValidateRange(string code, decimal value, decimal? min_value, decimal? max_value)
    {
        if (min_value.HasValue && value < min_value.Value)
            throw new ArgumentException($"Giá trị tham số '{code}' phải lớn hơn hoặc bằng {min_value.Value:N0}.");
        if (max_value.HasValue && value > max_value.Value)
            throw new ArgumentException($"Giá trị tham số '{code}' phải nhỏ hơn hoặc bằng {max_value.Value:N0}.");
    }

    private static bool TryParseDecimal(string raw_value, out decimal result)
    {
        string normalized = raw_value.Trim().Replace(" ", string.Empty);
        return decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out result)
               || decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.CurrentCulture, out result);
    }
}

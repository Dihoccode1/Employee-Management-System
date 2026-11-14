using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DAO.HumanResources.Positions;
using DTO.HumanResources.Positions;

namespace BUS.HumanResources.Positions;

public sealed class PositionService(IPositionDao dao)
{
    public Task<List<PositionDto>> GetAllAsync(CancellationToken cancellation_token = default)
        => dao.GetAllAsync(cancellation_token);

    public async Task<string> AddAsync(PositionDto item, CancellationToken cancellation_token = default)
    {
        ValidateAndNormalize(item);
        if (await dao.ExistsAsync(item.Code, cancellation_token))
            throw new InvalidOperationException($"Mã chức vụ '{item.Code}' đã tồn tại. Vui lòng nhập mã khác.");

        await dao.InsertAsync(item, cancellation_token);
        return item.Code;
    }

    public async Task<string> UpdateAsync(PositionDto item, CancellationToken cancellation_token = default)
    {
        ValidateAndNormalize(item);
        if (!await dao.ExistsAsync(item.Code, cancellation_token))
            throw new InvalidOperationException($"Không tìm thấy chức vụ '{item.Code}' để cập nhật.");

        await dao.UpdateAsync(item, cancellation_token);
        return item.Code;
    }

    public Task<string> SaveAsync(PositionDto item, bool is_edit_mode = false, CancellationToken cancellation_token = default)
        => is_edit_mode ? UpdateAsync(item, cancellation_token) : AddAsync(item, cancellation_token);

    public Task DeleteAsync(string code, CancellationToken cancellation_token = default)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Vui lòng chọn mã chức vụ cần xóa.");
        return dao.DeleteAsync(code.Trim(), cancellation_token);
    }

    public ArrayList Search(ArrayList source, string? term)
    {
        var result = new ArrayList();
        string keyword = term?.Trim() ?? string.Empty;
        foreach (object? obj in source)
        {
            if (obj is not PositionDto item)
                continue;

            if (keyword.Length == 0
                || item.Code.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                || item.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                || item.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                result.Add(item);
            }
        }
        return result;
    }

    private static void ValidateAndNormalize(PositionDto item)
    {
        ArgumentNullException.ThrowIfNull(item);
        item.Code = item.Code?.Trim() ?? string.Empty;
        item.Name = item.Name?.Trim() ?? string.Empty;
        item.Description = item.Description?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(item.Code) || item.Code.Length > 20)
            throw new ArgumentException("Mã chức vụ bắt buộc và tối đa 20 ký tự.");
        if (string.IsNullOrWhiteSpace(item.Name) || item.Name.Length > 100)
            throw new ArgumentException("Tên chức vụ bắt buộc và tối đa 100 ký tự.");
        if (item.Description.Length > 500)
            throw new ArgumentException("Mô tả chức vụ tối đa 500 ký tự.");
        if (item.AllowanceAmount < 0 || item.AllowanceAmount != decimal.Round(item.AllowanceAmount, 2))
            throw new ArgumentException("Phụ cấp chức vụ phải lớn hơn hoặc bằng 0 và tối đa 2 chữ số thập phân.");
    }
}

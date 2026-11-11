using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DAO.HumanResources.Positions;
using DTO.HumanResources.Positions;
namespace BUS.HumanResources.Positions;
public sealed class PositionService(IPositionDao dao) {
    public Task<List<PositionDto>> GetAllAsync() => dao.GetAllAsync();
    public async Task<string> SaveAsync(PositionDto item) {
        ArgumentNullException.ThrowIfNull(item);
        item.Code = item.Code?.Trim() ?? string.Empty;
        item.Name = item.Name?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(item.Code) || item.Code.Length > 20) throw new ArgumentException("Mã chức vụ bắt buộc và tối đa 20 ký tự.");
        if (string.IsNullOrWhiteSpace(item.Name) || item.Name.Length > 100) throw new ArgumentException("Tên chức vụ bắt buộc và tối đa 100 ký tự.");
        if (await dao.ExistsAsync(item.Code)) await dao.UpdateAsync(item);
        else await dao.InsertAsync(item);
        return item.Code;
    }
    public Task DeleteAsync(string code) {
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Vui lòng chọn mã chức vụ cần xóa.");
        return dao.DeleteAsync(code.Trim());
    }
    public ArrayList Search(ArrayList source, string? term) {
        var result = new ArrayList();
        string kw = term?.Trim() ?? string.Empty;
        foreach (object? obj in source) {
            if (obj is not PositionDto item) continue;
            if (kw.Length == 0 || item.Code.Contains(kw, StringComparison.OrdinalIgnoreCase) || item.Name.Contains(kw, StringComparison.OrdinalIgnoreCase))
                result.Add(item);
        }
        return result;
    }
}

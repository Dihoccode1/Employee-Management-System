using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DAO.HumanResources.Positions;
using DTO.HumanResources.Positions;
namespace BUS.HumanResources.Positions;
public sealed class PositionService(IPositionDao dao) {
    public Task<List<PositionDto>> GetAllAsync() => dao.GetAllAsync();
    public async Task<int> SaveAsync(PositionDto item) {
        ArgumentNullException.ThrowIfNull(item);
        if (item.Id < 0) throw new ArgumentException("Id không hợp lệ.");
        item.Code = item.Code?.Trim() ?? string.Empty;
        item.Name = item.Name?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(item.Code) || item.Code.Length > 20) throw new ArgumentException("Mã chức vụ tối đa 20 ký tự.");
        if (string.IsNullOrWhiteSpace(item.Name) || item.Name.Length > 100) throw new ArgumentException("Tên chức vụ tối đa 100 ký tự.");
        if (item.Id == 0) return await dao.InsertAsync(item);
        await dao.UpdateAsync(item);
        return item.Id;
    }
    public Task DeleteAsync(int id) {
        if (id <= 0) throw new ArgumentException("Id không hợp lệ.");
        return dao.DeleteAsync(id);
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

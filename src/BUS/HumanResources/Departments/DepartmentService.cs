using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DAO.HumanResources.Departments;
using DTO.HumanResources.Departments;
namespace BUS.HumanResources.Departments;
public sealed class DepartmentService(IDepartmentDao dao) {
    public Task<List<DepartmentDto>> GetAllAsync() => dao.GetAllAsync();
    public async Task<int> SaveAsync(DepartmentDto item) {
        ArgumentNullException.ThrowIfNull(item);
        if (item.Id < 0) throw new ArgumentException("Id không hợp lệ.");
        item.Code = item.Code?.Trim() ?? string.Empty;
        item.Name = item.Name?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(item.Code) || item.Code.Length > 20) throw new ArgumentException("Mã phòng ban tối đa 20 ký tự.");
        if (string.IsNullOrWhiteSpace(item.Name) || item.Name.Length > 100) throw new ArgumentException("Tên phòng ban tối đa 100 ký tự.");
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
            if (obj is not DepartmentDto item) continue;
            if (kw.Length == 0 || item.Code.Contains(kw, StringComparison.OrdinalIgnoreCase) || item.Name.Contains(kw, StringComparison.OrdinalIgnoreCase))
                result.Add(item);
        }
        return result;
    }
}

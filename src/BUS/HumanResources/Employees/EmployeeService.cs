using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DAO.HumanResources.Employees;
using DTO.HumanResources.Employees;
namespace BUS.HumanResources.Employees;
public sealed class EmployeeService(IEmployeeDao dao) {
    public Task<List<EmployeeDto>> GetAllAsync(CancellationToken cancellation_token = default)
        => dao.GetAllAsync(cancellation_token);
    public async Task<int> SaveAsync(EmployeeDto item) {
        ArgumentNullException.ThrowIfNull(item);
        if (item.Id < 0) throw new ArgumentException("Id không hợp lệ.");
        item.EmployeeCode = item.EmployeeCode?.Trim() ?? string.Empty;
        item.FullName = item.FullName?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(item.EmployeeCode) || item.EmployeeCode.Length > 20) throw new ArgumentException("Mã nhân viên bắt buộc.");
        if (string.IsNullOrWhiteSpace(item.FullName) || item.FullName.Length > 150) throw new ArgumentException("Họ tên bắt buộc.");
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
            if (obj is not EmployeeDto item) continue;
            if (kw.Length == 0 || item.EmployeeCode.Contains(kw, StringComparison.OrdinalIgnoreCase) || item.FullName.Contains(kw, StringComparison.OrdinalIgnoreCase) || item.DepartmentName.Contains(kw, StringComparison.OrdinalIgnoreCase))
                result.Add(item);
        }
        return result;
    }
}

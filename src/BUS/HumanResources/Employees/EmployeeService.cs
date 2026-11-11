using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DAO.HumanResources.Employees;
using DTO.HumanResources.Employees;
namespace BUS.HumanResources.Employees;
public sealed class EmployeeService(IEmployeeDao dao) {
    public Task<List<EmployeeDto>> GetAllAsync() => dao.GetAllAsync();
    public async Task<string> SaveAsync(EmployeeDto item) {
        ArgumentNullException.ThrowIfNull(item);
        item.EmployeeCode = item.EmployeeCode?.Trim() ?? string.Empty;
        item.FullName = item.FullName?.Trim() ?? string.Empty;
        item.Email = string.IsNullOrWhiteSpace(item.Email) ? null : item.Email.Trim();
        item.DepartmentCode = item.DepartmentCode?.Trim() ?? string.Empty;
        item.PositionCode = item.PositionCode?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(item.EmployeeCode) || item.EmployeeCode.Length > 20) throw new ArgumentException("Mã nhân viên bắt buộc và tối đa 20 ký tự.");
        if (string.IsNullOrWhiteSpace(item.FullName) || item.FullName.Length > 150) throw new ArgumentException("Họ tên bắt buộc và tối đa 150 ký tự.");
        if (await dao.ExistsAsync(item.EmployeeCode)) await dao.UpdateAsync(item);
        else await dao.InsertAsync(item);
        return item.EmployeeCode;
    }
    public Task DeleteAsync(string employeeCode) {
        if (string.IsNullOrWhiteSpace(employeeCode)) throw new ArgumentException("Vui lòng chọn mã nhân viên cần xóa.");
        return dao.DeleteAsync(employeeCode.Trim());
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

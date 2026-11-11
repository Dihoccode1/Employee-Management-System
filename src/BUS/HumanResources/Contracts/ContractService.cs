using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DAO.HumanResources.Contracts;
using DTO.HumanResources.Contracts;
namespace BUS.HumanResources.Contracts;
public sealed class ContractService(IContractDao dao) {
    public Task<List<ContractDto>> GetAllAsync() => dao.GetAllAsync();
    public async Task<string> SaveAsync(ContractDto item) {
        ArgumentNullException.ThrowIfNull(item);
        item.ContractCode = item.ContractCode?.Trim() ?? string.Empty;
        item.EmployeeCode = item.EmployeeCode?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(item.ContractCode) || item.ContractCode.Length > 30) throw new ArgumentException("Mã hợp đồng bắt buộc và tối đa 30 ký tự.");
        if (string.IsNullOrWhiteSpace(item.EmployeeCode)) throw new ArgumentException("Mã nhân viên bắt buộc.");
        if (item.BaseSalary < 0) throw new ArgumentException("Lương cơ bản không hợp lệ.");
        if (item.EndDate.HasValue && item.EndDate.Value.Date < item.StartDate.Date) throw new ArgumentException("Ngày kết thúc không hợp lệ.");
        if (await dao.ExistsAsync(item.ContractCode)) await dao.UpdateAsync(item);
        else await dao.InsertAsync(item);
        return item.ContractCode;
    }
    public Task DeleteAsync(string contractCode) {
        if (string.IsNullOrWhiteSpace(contractCode)) throw new ArgumentException("Vui lòng chọn mã hợp đồng cần xóa.");
        return dao.DeleteAsync(contractCode.Trim());
    }
    public ArrayList Search(ArrayList source, string? term) {
        var result = new ArrayList();
        string kw = term?.Trim() ?? string.Empty;
        foreach (object? obj in source) {
            if (obj is not ContractDto item) continue;
            if (kw.Length == 0 || item.ContractCode.Contains(kw, StringComparison.OrdinalIgnoreCase) || item.EmployeeCode.Contains(kw, StringComparison.OrdinalIgnoreCase) || item.EmployeeName.Contains(kw, StringComparison.OrdinalIgnoreCase))
                result.Add(item);
        }
        return result;
    }
}

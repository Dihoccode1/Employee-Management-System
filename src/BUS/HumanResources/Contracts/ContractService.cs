using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DAO.HumanResources.Contracts;
using DTO.HumanResources.Contracts;
namespace BUS.HumanResources.Contracts;
public sealed class ContractService(IContractDao dao) {
    public Task<List<ContractDto>> GetAllAsync() => dao.GetAllAsync();
    public async Task<int> SaveAsync(ContractDto item) {
        ArgumentNullException.ThrowIfNull(item);
        if (item.Id < 0 || item.EmployeeId <= 0) throw new ArgumentException("Dữ liệu không hợp lệ.");
        item.ContractCode = item.ContractCode?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(item.ContractCode)) throw new ArgumentException("Mã hợp đồng bắt buộc.");
        if (item.BaseSalary < 0) throw new ArgumentException("Lương cơ bản không hợp lệ.");
        if (item.EndDate.HasValue && item.EndDate.Value.Date < item.StartDate.Date) throw new ArgumentException("Ngày kết thúc không hợp lệ.");
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
            if (obj is not ContractDto item) continue;
            if (kw.Length == 0 || item.ContractCode.Contains(kw, StringComparison.OrdinalIgnoreCase) || item.EmployeeName.Contains(kw, StringComparison.OrdinalIgnoreCase))
                result.Add(item);
        }
        return result;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using DAO.HumanResources.Contracts;
using DAO.Settings;
using DTO.HumanResources.Contracts;
using DTO.Settings;

namespace BUS.HumanResources.Contracts;

/// <summary>Nghiệp vụ Hợp đồng lao động: tách thêm/sửa, kiểm tra lương tối thiểu và lọc theo nhân viên trên ArrayList.</summary>
public sealed class ContractService(IContractDao dao, ISystemParameterDao? parameter_dao = null)
{
    public Task<List<ContractDto>> GetAllAsync(CancellationToken cancellation_token = default)
        => dao.GetAllAsync(cancellation_token);

    public async Task<string> AddAsync(ContractDto item, CancellationToken cancellation_token = default)
    {
        ValidateAndNormalize(item);
        if (await dao.ExistsAsync(item.ContractCode, cancellation_token))
            throw new InvalidOperationException($"Mã hợp đồng '{item.ContractCode}' đã tồn tại. Vui lòng nhập mã khác.");

        await ValidateMinimumBaseSalaryAsync(item.BaseSalary, cancellation_token);
        await dao.InsertAsync(item, cancellation_token);
        return item.ContractCode;
    }

    public async Task<string> UpdateAsync(ContractDto item, CancellationToken cancellation_token = default)
    {
        ValidateAndNormalize(item);
        if (!await dao.ExistsAsync(item.ContractCode, cancellation_token))
            throw new InvalidOperationException($"Không tìm thấy hợp đồng '{item.ContractCode}' để cập nhật.");

        await ValidateMinimumBaseSalaryAsync(item.BaseSalary, cancellation_token);
        await dao.UpdateAsync(item, cancellation_token);
        return item.ContractCode;
    }

    public Task<string> SaveAsync(ContractDto item, bool is_edit_mode = false, CancellationToken cancellation_token = default)
        => is_edit_mode ? UpdateAsync(item, cancellation_token) : AddAsync(item, cancellation_token);

    public Task DeleteAsync(string contract_code, CancellationToken cancellation_token = default)
    {
        if (string.IsNullOrWhiteSpace(contract_code))
            throw new ArgumentException("Vui lòng chọn mã hợp đồng cần xóa.");
        return dao.DeleteAsync(contract_code.Trim(), cancellation_token);
    }

    public ArrayList Search(ArrayList source, string? term)
    {
        var result = new ArrayList();
        string keyword = term?.Trim() ?? string.Empty;
        foreach (object? obj in source)
        {
            if (obj is not ContractDto item)
                continue;

            if (keyword.Length == 0
                || item.ContractCode.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                || item.EmployeeCode.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                || item.EmployeeName.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                || item.ContractType.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                || item.Status.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                result.Add(item);
            }
        }
        return result;
    }

    /// <summary>Lọc hợp đồng theo nhân viên đang chọn trên màn hình Main–Sub từ danh sách ArrayList.</summary>
    public ArrayList SearchByEmployee(ArrayList source, int employee_id, string? employee_code, string? term = null)
    {
        var result = new ArrayList();
        string normalized_code = employee_code?.Trim() ?? string.Empty;
        if (employee_id <= 0 && normalized_code.Length == 0)
            return result;

        string keyword = term?.Trim() ?? string.Empty;
        foreach (object? obj in source)
        {
            if (obj is not ContractDto item)
                continue;

            bool matches_employee = (employee_id > 0 && item.EmployeeId == employee_id)
                || (normalized_code.Length > 0 && string.Equals(item.EmployeeCode, normalized_code, StringComparison.OrdinalIgnoreCase));

            if (!matches_employee)
                continue;

            if (keyword.Length == 0
                || item.ContractCode.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                || item.ContractType.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                || item.Status.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                result.Add(item);
            }
        }

        return result;
    }

    private async Task ValidateMinimumBaseSalaryAsync(decimal base_salary, CancellationToken cancellation_token)
    {
        if (parameter_dao is null || base_salary <= 0)
            return;

        SystemParameterDto? min_salary_param = await parameter_dao.GetByCodeAsync("MIN_BASE_SALARY", cancellation_token);
        if (min_salary_param is null)
            return;

        if (decimal.TryParse(min_salary_param.Value.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out decimal min_salary)
            && base_salary < min_salary)
        {
            throw new ArgumentException(
                $"Lương cơ bản hợp đồng ({base_salary:N0}) không được thấp hơn mức lương cơ bản tối thiểu ({min_salary:N0}).");
        }
    }

    private static void ValidateAndNormalize(ContractDto item)
    {
        ArgumentNullException.ThrowIfNull(item);
        item.ContractCode = item.ContractCode?.Trim() ?? string.Empty;
        item.EmployeeCode = item.EmployeeCode?.Trim() ?? string.Empty;
        item.ContractType = string.IsNullOrWhiteSpace(item.ContractType) ? "Xác định thời hạn" : item.ContractType.Trim();
        item.Status = string.IsNullOrWhiteSpace(item.Status) ? "Còn hiệu lực" : item.Status.Trim();

        if (string.IsNullOrWhiteSpace(item.ContractCode) || item.ContractCode.Length > 30)
            throw new ArgumentException("Mã hợp đồng bắt buộc và tối đa 30 ký tự.");
        if (item.EmployeeId <= 0 && string.IsNullOrWhiteSpace(item.EmployeeCode))
            throw new ArgumentException("Vui lòng chọn nhân viên cho hợp đồng.");
        if (item.ContractType.Length > 100)
            throw new ArgumentException("Loại hợp đồng tối đa 100 ký tự.");
        if (item.BaseSalary < 0 || item.BaseSalary != decimal.Round(item.BaseSalary, 2))
            throw new ArgumentException("Lương cơ bản phải >= 0 và tối đa 2 chữ số thập phân.");
        if (item.EndDate.HasValue && item.EndDate.Value.Date < item.StartDate.Date)
            throw new ArgumentException("Ngày kết thúc hợp đồng không được nhỏ hơn ngày bắt đầu.");
        if (item.Status.Length > 50)
            throw new ArgumentException("Trạng thái hợp đồng tối đa 50 ký tự.");
    }
}

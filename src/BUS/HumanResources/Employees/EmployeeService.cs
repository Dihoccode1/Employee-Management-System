using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using DAO.HumanResources.Employees;
using DAO.Settings;
using DTO.HumanResources.Employees;
using DTO.Settings;

namespace BUS.HumanResources.Employees;

/// <summary>Nghiệp vụ Nhân viên: tách thêm/sửa, kiểm tra tham số lương tối thiểu, tìm kiếm cơ bản & nâng cao trên ArrayList.</summary>
public sealed class EmployeeService(IEmployeeDao dao, ISystemParameterDao? parameter_dao = null)
{
    private static readonly string[] _date_formats =
    [
        "dd/MM/yyyy", "d/M/yyyy", "yyyy-MM-dd", "dd-MM-yyyy", "d-M-yyyy"
    ];

    public Task<List<EmployeeDto>> GetAllAsync(CancellationToken cancellation_token = default)
        => dao.GetAllAsync(cancellation_token);

    public async Task<string> AddAsync(EmployeeDto item, CancellationToken cancellation_token = default)
    {
        ValidateAndNormalize(item);
        if (await dao.ExistsAsync(item.EmployeeCode, cancellation_token))
            throw new InvalidOperationException($"Mã nhân viên '{item.EmployeeCode}' đã tồn tại. Vui lòng nhập mã khác.");

        await ValidateMinimumBaseSalaryAsync(item.CurrentBaseSalary, cancellation_token);
        await dao.InsertAsync(item, cancellation_token);
        return item.EmployeeCode;
    }

    public async Task<string> UpdateAsync(EmployeeDto item, CancellationToken cancellation_token = default)
    {
        ValidateAndNormalize(item);
        if (!await dao.ExistsAsync(item.EmployeeCode, cancellation_token))
            throw new InvalidOperationException($"Không tìm thấy hồ sơ nhân viên '{item.EmployeeCode}' để cập nhật.");

        await ValidateMinimumBaseSalaryAsync(item.CurrentBaseSalary, cancellation_token);
        await dao.UpdateAsync(item, cancellation_token);
        return item.EmployeeCode;
    }

    public Task<string> SaveAsync(EmployeeDto item, bool is_edit_mode = false, CancellationToken cancellation_token = default)
        => is_edit_mode ? UpdateAsync(item, cancellation_token) : AddAsync(item, cancellation_token);

    public Task DeleteAsync(string employee_code, CancellationToken cancellation_token = default)
    {
        if (string.IsNullOrWhiteSpace(employee_code))
            throw new ArgumentException("Vui lòng chọn mã nhân viên cần xóa.");
        return dao.DeleteAsync(employee_code.Trim(), cancellation_token);
    }

    public ArrayList Search(ArrayList source, string? term)
    {
        var result = new ArrayList();
        string keyword = term?.Trim() ?? string.Empty;
        foreach (object? obj in source)
        {
            if (obj is not EmployeeDto item)
                continue;

            if (keyword.Length == 0
                || item.EmployeeCode.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                || item.FullName.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                || item.DepartmentCode.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                || item.DepartmentName.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                || item.PositionName.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                || item.PhoneNumber.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                result.Add(item);
            }
        }
        return result;
    }

    /// <summary>Tìm kiếm nâng cao trên ArrayList với tổ hợp điều kiện AND, OR, NOT và kiểm tra kiểu dữ liệu từng trường.</summary>
    public ArrayList SearchAdvanced(ArrayList source, IReadOnlyList<EmployeeSearchCondition> conditions)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (conditions is null || conditions.Count == 0)
            return new ArrayList(source);

        // Kiểm tra định dạng giá trị và toán tử cho toàn bộ điều kiện trước khi lọc.
        var parsed_predicates = new List<(string logic, Func<EmployeeDto, bool> matcher)>(conditions.Count);
        foreach (EmployeeSearchCondition condition in conditions)
        {
            string logic = (condition.LogicalOperator?.Trim() ?? "AND").ToUpperInvariant();
            if (logic is not ("AND" or "OR" or "NOT"))
                throw new ArgumentException($"Phép kết hợp '{condition.LogicalOperator}' không hợp lệ (chỉ dùng AND, OR hoặc NOT).");

            Func<EmployeeDto, bool> matcher = BuildFieldMatcher(condition);
            parsed_predicates.Add((logic, matcher));
        }

        var result = new ArrayList();
        foreach (object? obj in source)
        {
            if (obj is not EmployeeDto employee)
                continue;

            bool combined = false;
            for (int index = 0; index < parsed_predicates.Count; index++)
            {
                (string logic, Func<EmployeeDto, bool> matcher) = parsed_predicates[index];
                bool matched = matcher(employee);

                if (index == 0)
                {
                    combined = logic == "NOT" ? !matched : matched;
                    continue;
                }

                combined = logic switch
                {
                    "AND" => combined && matched,
                    "OR" => combined || matched,
                    "NOT" => combined && !matched,
                    _ => combined
                };
            }

            if (combined)
                result.Add(employee);
        }

        return result;
    }

    private async Task ValidateMinimumBaseSalaryAsync(decimal current_base_salary, CancellationToken cancellation_token)
    {
        if (parameter_dao is null || current_base_salary <= 0)
            return;

        SystemParameterDto? min_salary_param = await parameter_dao.GetByCodeAsync("MIN_BASE_SALARY", cancellation_token);
        if (min_salary_param is null)
            return;

        if (decimal.TryParse(min_salary_param.Value.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out decimal min_salary)
            && current_base_salary < min_salary)
        {
            throw new ArgumentException(
                $"Lương cơ bản hiện hành ({current_base_salary:N0}) không được nhỏ hơn mức lương cơ bản tối thiểu theo tham số hệ thống ({min_salary:N0}).");
        }
    }

    private static void ValidateAndNormalize(EmployeeDto item)
    {
        ArgumentNullException.ThrowIfNull(item);
        item.EmployeeCode = item.EmployeeCode?.Trim() ?? string.Empty;
        item.SynchronizeNameFields();
        item.Gender = string.IsNullOrWhiteSpace(item.Gender) ? "Nam" : item.Gender.Trim();
        item.PhoneNumber = item.PhoneNumber?.Trim() ?? string.Empty;
        item.Email = string.IsNullOrWhiteSpace(item.Email) ? null : item.Email.Trim();
        item.DepartmentCode = item.DepartmentCode?.Trim() ?? string.Empty;
        item.PositionCode = item.PositionCode?.Trim() ?? string.Empty;
        item.Status = string.IsNullOrWhiteSpace(item.Status)
            ? (item.IsActive ? "Đang làm việc" : "Đã nghỉ việc")
            : item.Status.Trim();
        item.IsActive = !item.Status.Equals("Đã nghỉ việc", StringComparison.OrdinalIgnoreCase);
        item.Degree = string.IsNullOrWhiteSpace(item.Degree) ? null : item.Degree.Trim();

        if (string.IsNullOrWhiteSpace(item.EmployeeCode) || item.EmployeeCode.Length > 20)
            throw new ArgumentException("Mã nhân viên bắt buộc và tối đa 20 ký tự.");
        if (string.IsNullOrWhiteSpace(item.FullName) || item.FullName.Length > 150)
            throw new ArgumentException("Họ và tên nhân viên bắt buộc và tối đa 150 ký tự.");
        if (item.LastName.Length > 100 || item.FirstName.Length > 50)
            throw new ArgumentException("Họ tối đa 100 ký tự, Tên tối đa 50 ký tự.");
        if (item.DateOfBirth == default || item.DateOfBirth.Date > DateTime.Today.AddYears(-15))
            throw new ArgumentException("Ngày sinh không hợp lệ (nhân viên phải từ 15 tuổi trở lên).");
        if (item.HireDate == default)
            throw new ArgumentException("Ngày vào làm không hợp lệ.");
        if (item.HireDate.Date < item.DateOfBirth.Date)
            throw new ArgumentException("Ngày vào làm không được nhỏ hơn ngày sinh.");
        if (item.PhoneNumber.Length > 20)
            throw new ArgumentException("Số điện thoại tối đa 20 ký tự.");
        if (item.Email is not null && (item.Email.Length > 254 || !item.Email.Contains('@')))
            throw new ArgumentException("Địa chỉ email không hợp lệ.");
        if (item.CurrentBaseSalary < 0 || item.CurrentBaseSalary != decimal.Round(item.CurrentBaseSalary, 2))
            throw new ArgumentException("Lương cơ bản hiện hành phải >= 0 và tối đa 2 chữ số thập phân.");
        if (item.Degree is not null && item.Degree.Length > 100)
            throw new ArgumentException("Bằng cấp tối đa 100 ký tự.");
    }

    private static Func<EmployeeDto, bool> BuildFieldMatcher(EmployeeSearchCondition condition)
    {
        string field = condition.FieldName?.Trim() ?? string.Empty;
        string op = (condition.ComparisonOperator?.Trim() ?? "=").ToUpperInvariant();
        string raw_value = condition.Value?.Trim() ?? string.Empty;

        return field switch
        {
            nameof(EmployeeDto.HireDate) or nameof(EmployeeDto.DateOfBirth) => BuildDateMatcher(field, op, raw_value),
            nameof(EmployeeDto.CurrentBaseSalary) => BuildDecimalMatcher(field, op, raw_value),
            nameof(EmployeeDto.EmployeeCode) => BuildStringMatcher(e => e.EmployeeCode, field, op, raw_value),
            nameof(EmployeeDto.LastName) => BuildStringMatcher(e => e.LastName, field, op, raw_value),
            nameof(EmployeeDto.FirstName) => BuildStringMatcher(e => e.FirstName, field, op, raw_value),
            nameof(EmployeeDto.FullName) => BuildStringMatcher(e => e.FullName, field, op, raw_value),
            nameof(EmployeeDto.Gender) => BuildStringMatcher(e => e.Gender, field, op, raw_value),
            nameof(EmployeeDto.PhoneNumber) => BuildStringMatcher(e => e.PhoneNumber, field, op, raw_value),
            nameof(EmployeeDto.Email) => BuildStringMatcher(e => e.Email ?? string.Empty, field, op, raw_value),
            nameof(EmployeeDto.DepartmentCode) => BuildStringMatcher(e => e.DepartmentCode, field, op, raw_value),
            nameof(EmployeeDto.DepartmentName) => BuildStringMatcher(e => e.DepartmentName, field, op, raw_value),
            nameof(EmployeeDto.PositionCode) => BuildStringMatcher(e => e.PositionCode, field, op, raw_value),
            nameof(EmployeeDto.PositionName) => BuildStringMatcher(e => e.PositionName, field, op, raw_value),
            nameof(EmployeeDto.Status) => BuildStringMatcher(e => e.Status, field, op, raw_value),
            nameof(EmployeeDto.Degree) => BuildStringMatcher(e => e.Degree ?? string.Empty, field, op, raw_value),
            _ => throw new ArgumentException($"Trường tìm kiếm '{field}' không tồn tại trong hồ sơ nhân viên.")
        };
    }

    private static Func<EmployeeDto, bool> BuildStringMatcher(Func<EmployeeDto, string> selector, string field, string op, string value)
    {
        if (op is not ("=" or "<>" or "LIKE"))
            throw new ArgumentException($"Trường kiểu chuỗi '{field}' chỉ hỗ trợ toán tử '=', '<>' hoặc 'LIKE'.");

        return op switch
        {
            "=" => e => string.Equals(selector(e), value, StringComparison.OrdinalIgnoreCase),
            "<>" => e => !string.Equals(selector(e), value, StringComparison.OrdinalIgnoreCase),
            "LIKE" => e => selector(e).Contains(value, StringComparison.OrdinalIgnoreCase),
            _ => _ => false
        };
    }

    private static Func<EmployeeDto, bool> BuildDateMatcher(string field, string op, string raw_value)
    {
        if (op is not ("=" or "<>" or ">" or ">=" or "<" or "<="))
            throw new ArgumentException($"Trường ngày '{field}' chỉ hỗ trợ toán tử '=', '<>', '>', '>=', '<', '<='.");

        if (!DateTime.TryParseExact(raw_value, _date_formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime target_date))
            throw new ArgumentException($"Giá trị '{raw_value}' cho trường '{field}' không đúng định dạng ngày (ví dụ: 15/01/2025 hoặc 2025-01-15).");

        DateTime selector(EmployeeDto e) => field == nameof(EmployeeDto.DateOfBirth) ? e.DateOfBirth.Date : e.HireDate.Date;

        return op switch
        {
            "=" => e => selector(e) == target_date.Date,
            "<>" => e => selector(e) != target_date.Date,
            ">" => e => selector(e) > target_date.Date,
            ">=" => e => selector(e) >= target_date.Date,
            "<" => e => selector(e) < target_date.Date,
            "<=" => e => selector(e) <= target_date.Date,
            _ => _ => false
        };
    }

    private static Func<EmployeeDto, bool> BuildDecimalMatcher(string field, string op, string raw_value)
    {
        if (op is not ("=" or "<>" or ">" or ">=" or "<" or "<="))
            throw new ArgumentException($"Trường số '{field}' chỉ hỗ trợ toán tử '=', '<>', '>', '>=', '<', '<='.");

        string normalized = raw_value.Replace(",", string.Empty).Replace(".", string.Empty);
        bool parsed_invariant = decimal.TryParse(raw_value, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal target_number);
        if (!parsed_invariant && !decimal.TryParse(raw_value, NumberStyles.Number, CultureInfo.CurrentCulture, out target_number))
        {
            if (!decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out target_number))
                throw new ArgumentException($"Giá trị '{raw_value}' cho trường '{field}' phải là số hợp lệ.");
        }

        return op switch
        {
            "=" => e => e.CurrentBaseSalary == target_number,
            "<>" => e => e.CurrentBaseSalary != target_number,
            ">" => e => e.CurrentBaseSalary > target_number,
            ">=" => e => e.CurrentBaseSalary >= target_number,
            "<" => e => e.CurrentBaseSalary < target_number,
            "<=" => e => e.CurrentBaseSalary <= target_number,
            _ => _ => false
        };
    }
}

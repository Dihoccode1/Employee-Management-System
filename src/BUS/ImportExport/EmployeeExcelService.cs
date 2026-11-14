using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BUS.HumanResources.Departments;
using BUS.HumanResources.Employees;
using BUS.HumanResources.Positions;
using DTO.HumanResources.Departments;
using DTO.HumanResources.Employees;
using DTO.HumanResources.Positions;

namespace BUS.ImportExport;

/// <summary>Nghiệp vụ nhập/xuất hồ sơ nhân viên bằng tệp Excel .xls đúng các cột ERD đã chốt.</summary>
public sealed class EmployeeExcelService(
    EmployeeService employee_service,
    DepartmentService department_service,
    PositionService position_service)
{
    private static readonly string[] _date_formats =
    [
        "dd/MM/yyyy", "d/M/yyyy", "yyyy-MM-dd", "dd-MM-yyyy", "d-M-yyyy", "M/d/yyyy"
    ];

    private static readonly string[] _export_headers =
    [
        "EmployeeCode",
        "LastName",
        "FirstName",
        "DateOfBirth",
        "Gender",
        "PhoneNumber",
        "Email",
        "DepartmentCode",
        "PositionCode",
        "HireDate",
        "CurrentBaseSalary",
        "Status",
        "Degree"
    ];

    public void ExportToXls(string file_path, ArrayList displayed_employees)
    {
        if (string.IsNullOrWhiteSpace(file_path))
            throw new ArgumentException("Đường dẫn tệp xuất Excel không hợp lệ.");

        ArgumentNullException.ThrowIfNull(displayed_employees);

        var rows = new List<IReadOnlyList<object?>> { _export_headers };
        foreach (object? obj in displayed_employees)
        {
            if (obj is not EmployeeDto emp)
                continue;

            emp.SynchronizeNameFields();
            rows.Add(
            [
                emp.EmployeeCode,
                emp.LastName,
                emp.FirstName,
                emp.DateOfBirth,
                emp.Gender,
                emp.PhoneNumber,
                emp.Email ?? string.Empty,
                emp.DepartmentCode,
                emp.PositionCode,
                emp.HireDate,
                emp.CurrentBaseSalary,
                emp.Status,
                emp.Degree ?? string.Empty
            ]);
        }

        XlsWorkbookCodec.WriteXls(file_path, "NhanVien", rows);
    }

    public async Task<EmployeeImportResultDto> ImportFromXlsAsync(string file_path, CancellationToken cancellation_token = default)
    {
        if (string.IsNullOrWhiteSpace(file_path) || !File.Exists(file_path))
            throw new ArgumentException("Không tìm thấy tệp Excel cần nhập.");

        List<List<string>> raw_rows = XlsWorkbookCodec.ReadXlsRows(file_path);
        var result = new EmployeeImportResultDto();
        if (raw_rows.Count <= 1)
        {
            result.Errors.Add("Tệp Excel không có dữ liệu nhân viên (cần ít nhất 1 dòng tiêu đề và 1 dòng dữ liệu).");
            return result;
        }

        Dictionary<string, int> column_map = BuildColumnIndexMap(raw_rows[0]);
        if (!column_map.ContainsKey("EmployeeCode") || (!column_map.ContainsKey("FirstName") && !column_map.ContainsKey("FullName")))
        {
            result.Errors.Add("Dòng 1: Thiếu cột bắt buộc (EmployeeCode, LastName/FirstName hoặc FullName).");
            return result;
        }

        List<EmployeeDto> existing_employees = await employee_service.GetAllAsync(cancellation_token);
        var existing_codes = new HashSet<string>(existing_employees.Select(e => e.EmployeeCode), StringComparer.OrdinalIgnoreCase);

        List<DepartmentDto> departments = await department_service.GetAllAsync(cancellation_token);
        var dept_by_code = departments.ToDictionary(d => d.Code, StringComparer.OrdinalIgnoreCase);

        List<PositionDto> positions = await position_service.GetAllAsync(cancellation_token);
        var pos_by_code = positions.ToDictionary(p => p.Code, StringComparer.OrdinalIgnoreCase);

        var file_codes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var valid_items = new List<EmployeeDto>();

        for (int row_index = 1; row_index < raw_rows.Count; row_index++)
        {
            List<string> row = raw_rows[row_index];
            if (row.All(string.IsNullOrWhiteSpace))
                continue;

            int excel_row = row_index + 1;
            result.TotalRows++;

            string emp_code = ReadCell(row, column_map, "EmployeeCode");
            if (string.IsNullOrWhiteSpace(emp_code))
            {
                result.Errors.Add($"Dòng {excel_row}: Mã nhân viên không được để trống.");
                continue;
            }
            if (emp_code.Length > 20)
            {
                result.Errors.Add($"Dòng {excel_row}: Mã nhân viên '{emp_code}' vượt quá 20 ký tự.");
                continue;
            }
            if (!file_codes.Add(emp_code))
            {
                result.Errors.Add($"Dòng {excel_row}: Trùng mã nhân viên '{emp_code}' trong tệp Excel.");
                continue;
            }
            if (existing_codes.Contains(emp_code))
            {
                result.Errors.Add($"Dòng {excel_row}: Mã nhân viên '{emp_code}' đã tồn tại trong CSDL.");
                continue;
            }

            string last_name = ReadCell(row, column_map, "LastName");
            string first_name = ReadCell(row, column_map, "FirstName");
            string full_name = ReadCell(row, column_map, "FullName");
            if (string.IsNullOrWhiteSpace(last_name) && string.IsNullOrWhiteSpace(first_name) && string.IsNullOrWhiteSpace(full_name))
            {
                result.Errors.Add($"Dòng {excel_row}: Họ và tên nhân viên không được để trống.");
                continue;
            }

            string dob_raw = ReadCell(row, column_map, "DateOfBirth");
            if (!TryParseDate(dob_raw, new DateTime(2000, 1, 1), out DateTime date_of_birth))
            {
                result.Errors.Add($"Dòng {excel_row}: Ngày sinh '{dob_raw}' không đúng định dạng ngày (dd/MM/yyyy hoặc yyyy-MM-dd).");
                continue;
            }

            string hire_raw = ReadCell(row, column_map, "HireDate");
            if (!TryParseDate(hire_raw, DateTime.Today, out DateTime hire_date))
            {
                result.Errors.Add($"Dòng {excel_row}: Ngày vào làm '{hire_raw}' không đúng định dạng ngày (dd/MM/yyyy hoặc yyyy-MM-dd).");
                continue;
            }

            string salary_raw = ReadCell(row, column_map, "CurrentBaseSalary");
            decimal salary = 0m;
            if (!string.IsNullOrWhiteSpace(salary_raw) && (!TryParseDecimal(salary_raw, out salary) || salary < 0))
            {
                result.Errors.Add($"Dòng {excel_row}: Lương cơ bản hiện hành '{salary_raw}' không phải số hợp lệ >= 0.");
                continue;
            }

            string dept_code = ReadCell(row, column_map, "DepartmentCode");
            DepartmentDto? matched_dept = null;
            if (!string.IsNullOrWhiteSpace(dept_code) && !dept_by_code.TryGetValue(dept_code, out matched_dept))
            {
                result.Errors.Add($"Dòng {excel_row}: Mã phòng ban '{dept_code}' không tồn tại.");
                continue;
            }

            string pos_code = ReadCell(row, column_map, "PositionCode");
            PositionDto? matched_pos = null;
            if (!string.IsNullOrWhiteSpace(pos_code) && !pos_by_code.TryGetValue(pos_code, out matched_pos))
            {
                result.Errors.Add($"Dòng {excel_row}: Mã chức vụ '{pos_code}' không tồn tại.");
                continue;
            }

            string status = ReadCell(row, column_map, "Status");
            if (string.IsNullOrWhiteSpace(status))
                status = "Đang làm việc";

            string gender = ReadCell(row, column_map, "Gender");
            if (string.IsNullOrWhiteSpace(gender))
                gender = "Nam";

            string email = ReadCell(row, column_map, "Email");
            string phone = ReadCell(row, column_map, "PhoneNumber");
            string degree = ReadCell(row, column_map, "Degree");

            var employee = new EmployeeDto
            {
                EmployeeCode = emp_code,
                LastName = last_name,
                FirstName = first_name,
                FullName = full_name,
                DateOfBirth = date_of_birth,
                Gender = gender,
                PhoneNumber = phone,
                Email = string.IsNullOrWhiteSpace(email) ? null : email,
                DepartmentId = matched_dept?.Id,
                DepartmentCode = matched_dept?.Code ?? string.Empty,
                DepartmentName = matched_dept?.Name ?? string.Empty,
                PositionId = matched_pos?.Id,
                PositionCode = matched_pos?.Code ?? string.Empty,
                PositionName = matched_pos?.Name ?? string.Empty,
                HireDate = hire_date,
                CurrentBaseSalary = salary,
                Status = status,
                IsActive = !status.Equals("Đã nghỉ việc", StringComparison.OrdinalIgnoreCase),
                Degree = string.IsNullOrWhiteSpace(degree) ? null : degree
            };
            employee.SynchronizeNameFields();
            valid_items.Add(employee);
        }

        // Chỉ lưu xuống CSDL khi toàn bộ dòng dữ liệu hợp lệ để tránh nhập dở dang.
        if (result.HasErrors)
            return result;

        foreach (EmployeeDto item in valid_items)
        {
            await employee_service.AddAsync(item, cancellation_token);
            result.ImportedCount++;
        }

        return result;
    }

    private static Dictionary<string, int> BuildColumnIndexMap(List<string> header_row)
    {
        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < header_row.Count; i++)
        {
            string normalized = header_row[i].Trim();
            string canonical = normalized.ToLowerInvariant() switch
            {
                "employeecode" or "ma_nhan_vien" or "mã nhân viên" or "mã nv" => "EmployeeCode",
                "lastname" or "ho" or "họ" => "LastName",
                "firstname" or "ten" or "tên" => "FirstName",
                "fullname" or "họ tên" or "họ và tên" => "FullName",
                "dateofbirth" or "ngay_sinh" or "ngày sinh" => "DateOfBirth",
                "gender" or "gioi_tinh" or "giới tính" => "Gender",
                "phonenumber" or "so_dien_thoai" or "số điện thoại" or "điện thoại" => "PhoneNumber",
                "email" => "Email",
                "departmentcode" or "ma_phong_ban" or "mã phòng ban" or "mã pb" => "DepartmentCode",
                "positioncode" or "ma_chuc_vu" or "mã chức vụ" or "mã cv" => "PositionCode",
                "hiredate" or "ngay_vao_lam" or "ngày vào làm" => "HireDate",
                "currentbasesalary" or "luong_co_ban_hien_hanh" or "lương cơ bản hiện hành" or "lương cơ bản" => "CurrentBaseSalary",
                "status" or "trang_thai" or "trạng thái" => "Status",
                "degree" or "bang_cap" or "bằng cấp" => "Degree",
                _ => normalized
            };

            map.TryAdd(canonical, i);
        }
        return map;
    }

    private static string ReadCell(List<string> row, Dictionary<string, int> column_map, string key)
    {
        if (!column_map.TryGetValue(key, out int col_index) || col_index < 0 || col_index >= row.Count)
            return string.Empty;
        return row[col_index].Trim();
    }

    private static bool TryParseDate(string raw, DateTime default_if_empty, out DateTime parsed)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            parsed = default_if_empty;
            return true;
        }

        if (DateTime.TryParseExact(raw.Trim(), _date_formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed))
            return true;

        return DateTime.TryParse(raw.Trim(), CultureInfo.CurrentCulture, DateTimeStyles.None, out parsed);
    }

    private static bool TryParseDecimal(string raw, out decimal value)
    {
        string cleaned = raw.Trim().Replace(" ", string.Empty);
        return decimal.TryParse(cleaned, NumberStyles.Number, CultureInfo.InvariantCulture, out value)
               || decimal.TryParse(cleaned, NumberStyles.Number, CultureInfo.CurrentCulture, out value);
    }
}

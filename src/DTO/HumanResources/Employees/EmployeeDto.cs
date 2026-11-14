using System;

namespace DTO.HumanResources.Employees;

/// <summary>Hồ sơ nhân viên đồng bộ cả khóa Id (liên kết FK) và EmployeeCode (mã nghiệp vụ) cùng các thuộc tính ERD.</summary>
public sealed class EmployeeDto
{
    public int Id { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; } = new DateTime(2000, 1, 1);
    public string Gender { get; set; } = "Nam";
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public int? DepartmentId { get; set; }
    public string DepartmentCode { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public int? PositionId { get; set; }
    public string PositionCode { get; set; } = string.Empty;
    public string PositionName { get; set; } = string.Empty;
    public DateTime HireDate { get; set; } = DateTime.Today;
    public string Status { get; set; } = "Đang làm việc";
    public bool IsActive { get; set; } = true;
    public decimal CurrentBaseSalary { get; set; }
    public string? Degree { get; set; }

    /// <summary>Đồng bộ qua lại giữa Họ/Tên (theo ERD) và Họ tên đầy đủ (dùng cho Dự án/Chấm công).</summary>
    public void SynchronizeNameFields()
    {
        LastName = LastName?.Trim() ?? string.Empty;
        FirstName = FirstName?.Trim() ?? string.Empty;
        FullName = FullName?.Trim() ?? string.Empty;

        // Nếu người dùng nhập Họ + Tên thì tự ghép FullName tương ứng.
        if (!string.IsNullOrEmpty(LastName) || !string.IsNullOrEmpty(FirstName))
        {
            FullName = string.Join(" ", new[] { LastName, FirstName }
                .Where(part => !string.IsNullOrWhiteSpace(part)));
            return;
        }

        // Ngược lại, nếu chỉ truyền FullName (từ API/module khác) thì tách thành Họ và Tên.
        if (!string.IsNullOrEmpty(FullName))
        {
            int last_space = FullName.LastIndexOf(' ');
            if (last_space > 0)
            {
                LastName = FullName[..last_space].Trim();
                FirstName = FullName[(last_space + 1)..].Trim();
            }
            else
            {
                LastName = string.Empty;
                FirstName = FullName;
            }
        }
    }
}

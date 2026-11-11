using System;
namespace DTO.HumanResources.Employees;
public sealed class EmployeeDto {
    public string EmployeeCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string DepartmentCode { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string PositionCode { get; set; } = string.Empty;
    public string PositionName { get; set; } = string.Empty;
    public DateTime HireDate { get; set; } = DateTime.Today;
    public bool IsActive { get; set; } = true;
}

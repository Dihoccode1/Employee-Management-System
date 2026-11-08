namespace DTO.HumanResources.Employees;
public sealed class EmployeeDto {
    public int Id { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public int? DepartmentId { get; set; }
    public int? PositionId { get; set; }
    public DateTime HireDate { get; set; } = DateTime.Today;
    public bool IsActive { get; set; } = true;
    public string DepartmentName { get; set; } = string.Empty;
    public string PositionName { get; set; } = string.Empty;
}

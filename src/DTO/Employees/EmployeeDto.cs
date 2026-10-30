namespace DTO.Employees;
public sealed class EmployeeDto
{
    public int Id { get; set; }
    public string EmployeeCode { get; set; } = "";
    public string FullName { get; set; } = "";
    public string? Email { get; set; }
    public int? DepartmentId { get; set; }
    public int? PositionId { get; set; }
    public DateTime HireDate { get; set; }
    public bool IsActive { get; set; }
}
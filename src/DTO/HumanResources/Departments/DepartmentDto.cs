namespace DTO.HumanResources.Departments;

/// <summary>Thông tin phòng ban theo ERD (bao gồm Id khóa thay thế và mã nghiệp vụ Code).</summary>
public sealed class DepartmentDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

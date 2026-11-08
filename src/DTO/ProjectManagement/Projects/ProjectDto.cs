namespace DTO.ProjectManagement.Projects;

// DTO chỉ mang dữ liệu giữa GUI, BUS và DAO.
public sealed class ProjectDto
{
    // Mã do SQL Server tự sinh, người dùng không nhập.
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime Deadline { get; set; }
    public string Description { get; set; } = "";
    public decimal Budget { get; set; }
    public int DepartmentId { get; set; }
    public int ManagerId { get; set; }
    public string Status { get; set; } = "";

    // Các tên này chỉ dùng để hiển thị, không ghi vào bảng.
    public string DepartmentName { get; set; } = "";
    public string ManagerName { get; set; } = "";
}

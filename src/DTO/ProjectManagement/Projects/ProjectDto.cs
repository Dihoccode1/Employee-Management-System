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

    // Các mã/tên này dùng để đồng bộ tra cứu và hiển thị, không ghi trực tiếp vào bảng Projects.
    public string DepartmentCode { get; set; } = "";
    public string DepartmentName { get; set; } = "";
    public string ManagerCode { get; set; } = "";
    public string ManagerName { get; set; } = "";
}

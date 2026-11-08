namespace DTO.ProjectManagement.Tasks;

// DTO chỉ mang dữ liệu giữa GUI, BUS và DAO.
public sealed class ProjectTaskDto
{
    // Mã do SQL Server tự sinh, người dùng không nhập.
    public int Id { get; set; }
    public int ProjectDetailId { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Type { get; set; } = "";
    public string Priority { get; set; } = "";
    public DateTime StartDate { get; set; }
    public DateTime Deadline { get; set; }
    public DateTime? CompletedDate { get; set; }
    public decimal EstimatedHours { get; set; }
    public decimal CompletionPercentage { get; set; }
    public string Status { get; set; } = "";
    public string Notes { get; set; } = "";

    // Các tên này chỉ dùng để hiển thị, không ghi vào bảng.
    public string ProjectName { get; set; } = "";
}

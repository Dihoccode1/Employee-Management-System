namespace DTO.ProjectManagement.Assignments;

// DTO chỉ mang dữ liệu giữa GUI, BUS và DAO.
public sealed class AssignmentDto
{
    // Mã do SQL Server tự sinh, người dùng không nhập.
    public int Id { get; set; }
    public int TaskId { get; set; }
    public int EmployeeId { get; set; }
    public DateTime AssignedDate { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime Deadline { get; set; }
    public DateTime? CompletedDate { get; set; }
    public decimal CompletionPercentage { get; set; }
    public decimal ContributionScore { get; set; }
    public string Status { get; set; } = "";
    public string Notes { get; set; } = "";

    // Các mã/tên này dùng để đồng bộ tra cứu và hiển thị, không ghi vào bảng.
    public string TaskName { get; set; } = "";
    public string EmployeeCode { get; set; } = "";
    public string EmployeeName { get; set; } = "";
    public string ProjectName { get; set; } = "";
}

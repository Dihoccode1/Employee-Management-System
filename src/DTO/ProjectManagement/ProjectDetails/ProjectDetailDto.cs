namespace DTO.ProjectManagement.ProjectDetails;

// DTO chỉ mang dữ liệu giữa GUI, BUS và DAO.
public sealed class ProjectDetailDto
{
    // Mã do SQL Server tự sinh, người dùng không nhập.
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public decimal CompletionPercentage { get; set; }
    public string Status { get; set; } = "";
    public decimal Allowance { get; set; }
    public decimal Bonus { get; set; }

    // Các tên này chỉ dùng để hiển thị, không ghi vào bảng.
    public string ProjectName { get; set; } = "";
}

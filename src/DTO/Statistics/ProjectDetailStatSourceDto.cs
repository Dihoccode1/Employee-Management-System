using System;

namespace DTO.Statistics;

/// <summary>Dòng dữ liệu Chi tiết dự án gắn với ProjectId và StartDate của dự án để tổng hợp phụ cấp/thưởng không bị trùng lặp.</summary>
public sealed class ProjectDetailStatSourceDto
{
    public int DetailId { get; set; }
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public DateTime ProjectStartDate { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public string ManagerName { get; set; } = string.Empty;
    public decimal Allowance { get; set; }
    public decimal Bonus { get; set; }
}

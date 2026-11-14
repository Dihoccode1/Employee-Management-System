namespace DTO.Statistics;

/// <summary>Một dòng kết quả thống kê (ngân sách, phụ cấp hoặc thưởng dự án theo kỳ).</summary>
public sealed class ProjectStatisticsRowDto
{
    public string PeriodName { get; set; } = string.Empty;
    public string CriterionLabel { get; set; } = string.Empty;
    public string GroupKey { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public int ItemCount { get; set; }
    public decimal Amount { get; set; }
}

using System;

namespace DTO.Statistics;

/// <summary>Tiêu chí lọc theo kỳ và nhóm tổng hợp cho thống kê dự án.</summary>
public sealed class ProjectStatisticsFilterDto
{
    /// <summary>ALL | DATE_RANGE | MONTH | QUARTER | YEAR</summary>
    public string PeriodType { get; set; } = "ALL";
    public DateTime FromDate { get; set; } = new DateTime(DateTime.Today.Year, 1, 1);
    public DateTime ToDate { get; set; } = DateTime.Today;
    public int Month { get; set; } = DateTime.Today.Month;
    public int Quarter { get; set; } = ((DateTime.Today.Month - 1) / 3) + 1;
    public int Year { get; set; } = DateTime.Today.Year;

    /// <summary>DEPARTMENT | MANAGER</summary>
    public string BudgetGroupBy { get; set; } = "DEPARTMENT";
}

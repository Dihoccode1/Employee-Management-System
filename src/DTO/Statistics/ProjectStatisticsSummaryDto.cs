using System;
using System.Collections.Generic;

namespace DTO.Statistics;

/// <summary>Tổng hợp dùng chung giữa màn hình thống kê và báo cáo PDF.</summary>
public sealed class ProjectStatisticsSummaryDto
{
    public string PeriodTitle { get; set; } = string.Empty;
    public string BudgetCriterionTitle { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; } = DateTime.Now;

    public List<ProjectStatisticsRowDto> BudgetRows { get; } = [];
    public decimal TotalBudget { get; set; }

    public List<ProjectStatisticsRowDto> AllowanceRows { get; } = [];
    public decimal TotalAllowance { get; set; }

    public List<ProjectStatisticsRowDto> BonusRows { get; } = [];
    public decimal TotalBonus { get; set; }
}

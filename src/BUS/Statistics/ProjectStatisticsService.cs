using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DAO.Statistics;
using DTO.ProjectManagement.Projects;
using DTO.Statistics;

namespace BUS.Statistics;

/// <summary>
/// Nghiệp vụ thống kê ngân sách, phụ cấp và thưởng dự án theo kỳ [D5, D6].
/// </summary>
public sealed class ProjectStatisticsService(IProjectStatisticsDao dao)
{
    public async Task<ProjectStatisticsSummaryDto> GetSummaryAsync(
        ProjectStatisticsFilterDto filter,
        CancellationToken cancellation_token = default)
    {
        ArgumentNullException.ThrowIfNull(filter);

        (DateTime? start_inclusive, DateTime? end_inclusive, string period_title) = ResolvePeriodRange(filter);

        List<ProjectDto> all_projects = await dao.GetProjectsAsync(cancellation_token);
        List<ProjectDetailStatSourceDto> all_details = await dao.GetProjectDetailsForStatsAsync(cancellation_token);

        return BuildSummary(filter, all_projects, all_details, start_inclusive, end_inclusive, period_title);
    }

    /// <summary>Tính toán trực tiếp 3 bảng thống kê (Ngân sách, Phụ cấp, Thưởng) từ dữ liệu đã truy vấn.</summary>
    public ProjectStatisticsSummaryDto BuildSummary(
        ProjectStatisticsFilterDto filter,
        IReadOnlyList<ProjectDto> projects,
        IReadOnlyList<ProjectDetailStatSourceDto> details)
    {
        ArgumentNullException.ThrowIfNull(filter);
        (DateTime? start_inclusive, DateTime? end_inclusive, string period_title) = ResolvePeriodRange(filter);
        return BuildSummary(filter, projects, details, start_inclusive, end_inclusive, period_title);
    }

    private static ProjectStatisticsSummaryDto BuildSummary(
        ProjectStatisticsFilterDto filter,
        IReadOnlyList<ProjectDto> projects,
        IReadOnlyList<ProjectDetailStatSourceDto> details,
        DateTime? start_inclusive,
        DateTime? end_inclusive,
        string period_title)
    {
        string group_by = (filter.BudgetGroupBy ?? "DEPARTMENT").Trim().ToUpperInvariant();
        if (group_by is not ("DEPARTMENT" or "MANAGER"))
            throw new ArgumentException("Tiêu chí nhóm ngân sách phải là Phòng ban (DEPARTMENT) hoặc Người quản lý (MANAGER).");

        string criterion_title = group_by == "DEPARTMENT" ? "Phòng ban" : "Người quản lý";
        var summary = new ProjectStatisticsSummaryDto
        {
            PeriodTitle = period_title,
            BudgetCriterionTitle = criterion_title,
            GeneratedAt = DateTime.Now
        };

        // 1. Thống kê tổng ngân sách dự án theo Phòng ban hoặc Người quản lý (lấy StartDate làm mốc kỳ).
        IEnumerable<ProjectDto> filtered_projects = projects.Where(p => IsInPeriod(p.StartDate, start_inclusive, end_inclusive));
        var budget_groups = filtered_projects
            .GroupBy(p => group_by == "DEPARTMENT"
                ? (Key: $"DEPT:{p.DepartmentId}", Name: string.IsNullOrWhiteSpace(p.DepartmentName) ? $"Phòng #{p.DepartmentId}" : p.DepartmentName)
                : (Key: $"MGR:{p.ManagerId}", Name: string.IsNullOrWhiteSpace(p.ManagerName) ? $"NV #{p.ManagerId}" : p.ManagerName))
            .OrderByDescending(g => g.Sum(x => x.Budget))
            .ThenBy(g => g.Key.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var grp in budget_groups)
        {
            decimal sum_budget = grp.Sum(x => x.Budget);
            summary.BudgetRows.Add(new ProjectStatisticsRowDto
            {
                PeriodName = period_title,
                CriterionLabel = criterion_title,
                GroupKey = grp.Key.Key,
                GroupName = grp.Key.Name,
                ItemCount = grp.Count(),
                Amount = sum_budget
            });
            summary.TotalBudget += sum_budget;
        }

        // 2 & 3. Thống kê phụ cấp và thưởng theo khóa ProjectId (từ Chi tiết dự án, tránh cộng trùng khi nối Công việc/Phân công).
        IEnumerable<ProjectDetailStatSourceDto> filtered_details = details
            .Where(d => IsInPeriod(d.ProjectStartDate, start_inclusive, end_inclusive));

        var detail_groups = filtered_details
            .GroupBy(d => (d.ProjectId, Name: string.IsNullOrWhiteSpace(d.ProjectName) ? $"Dự án #{d.ProjectId}" : d.ProjectName))
            .OrderBy(g => g.Key.ProjectId);

        foreach (var grp in detail_groups)
        {
            string project_label = $"#{grp.Key.ProjectId} - {grp.Key.Name}";
            decimal allowance_sum = grp.Sum(x => x.Allowance);
            decimal bonus_sum = grp.Sum(x => x.Bonus);
            int count = grp.Count();

            summary.AllowanceRows.Add(new ProjectStatisticsRowDto
            {
                PeriodName = period_title,
                CriterionLabel = "Dự án",
                GroupKey = grp.Key.ProjectId.ToString(CultureInfo.InvariantCulture),
                GroupName = project_label,
                ItemCount = count,
                Amount = allowance_sum
            });
            summary.TotalAllowance += allowance_sum;

            summary.BonusRows.Add(new ProjectStatisticsRowDto
            {
                PeriodName = period_title,
                CriterionLabel = "Dự án",
                GroupKey = grp.Key.ProjectId.ToString(CultureInfo.InvariantCulture),
                GroupName = project_label,
                ItemCount = count,
                Amount = bonus_sum
            });
            summary.TotalBonus += bonus_sum;
        }

        return summary;
    }

    private static bool IsInPeriod(DateTime target_date, DateTime? start_inclusive, DateTime? end_inclusive)
    {
        DateTime day = target_date.Date;
        if (start_inclusive.HasValue && day < start_inclusive.Value)
            return false;
        if (end_inclusive.HasValue && day > end_inclusive.Value)
            return false;
        return true;
    }

    private static (DateTime? Start, DateTime? End, string Title) ResolvePeriodRange(ProjectStatisticsFilterDto filter)
    {
        string period_type = (filter.PeriodType ?? "ALL").Trim().ToUpperInvariant();
        switch (period_type)
        {
            case "ALL":
                return (null, null, "Tất cả thời gian");

            case "DATE_RANGE":
            {
                DateTime from = filter.FromDate.Date;
                DateTime to = filter.ToDate.Date;
                if (to < from)
                    throw new ArgumentException("Ngày kết thúc kỳ lọc phải lớn hơn hoặc bằng ngày bắt đầu.");
                return (from, to, $"Từ {from:dd/MM/yyyy} đến {to:dd/MM/yyyy}");
            }

            case "MONTH":
            {
                ValidateYear(filter.Year);
                if (filter.Month < 1 || filter.Month > 12)
                    throw new ArgumentException("Tháng thống kê phải nằm trong khoảng từ 1 đến 12.");
                var from = new DateTime(filter.Year, filter.Month, 1);
                var to = from.AddMonths(1).AddDays(-1);
                return (from, to, $"Tháng {filter.Month:00}/{filter.Year}");
            }

            case "QUARTER":
            {
                ValidateYear(filter.Year);
                if (filter.Quarter < 1 || filter.Quarter > 4)
                    throw new ArgumentException("Quý thống kê phải nằm trong khoảng từ 1 đến 4.");
                int first_month = (filter.Quarter - 1) * 3 + 1;
                var from = new DateTime(filter.Year, first_month, 1);
                var to = from.AddMonths(3).AddDays(-1);
                return (from, to, $"Quý {filter.Quarter}/{filter.Year}");
            }

            case "YEAR":
            {
                ValidateYear(filter.Year);
                var from = new DateTime(filter.Year, 1, 1);
                var to = new DateTime(filter.Year, 12, 31);
                return (from, to, $"Năm {filter.Year}");
            }

            default:
                throw new ArgumentException($"Loại kỳ lọc '{filter.PeriodType}' không hợp lệ.");
        }
    }

    private static void ValidateYear(int year)
    {
        if (year < 1900 || year > 2200)
            throw new ArgumentException("Năm thống kê phải nằm trong khoảng 1900 - 2200.");
    }
}

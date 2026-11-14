using System.Collections;
using System.Globalization;
using DAO.ProjectManagement.Projects;
using DTO.ProjectManagement.Projects;

namespace BUS.ProjectManagement.Projects;

public sealed class ProjectService
{
    private static readonly string[] _date_formats =
    [
        "dd/MM/yyyy", "d/M/yyyy", "yyyy-MM-dd", "dd-MM-yyyy", "d-M-yyyy", "M/d/yyyy"
    ];

    private readonly IProjectDao _dao;

    public ProjectService(IProjectDao dao)
    {
        _dao = dao;
    }

    public Task<List<ProjectDto>> GetAllAsync()
    {
        return _dao.GetAllAsync();
    }

    public async Task<int> SaveAsync(ProjectDto item)
    {
        ArgumentNullException.ThrowIfNull(item);
        ProjectValidation.RecordId(item.Id);
        item.Status = ProjectValidation.RequiredText(item.Status, "trạng thái", 50);
        item.Name = ProjectValidation.RequiredText(item.Name, "tên dự án", 200);
        item.Description = ProjectValidation.OptionalText(item.Description, "mô tả", 2000);
        ProjectValidation.PositiveId(item.DepartmentId, "phòng ban");
        ProjectValidation.PositiveId(item.ManagerId, "người quản lý");
        ProjectValidation.Number(item.Budget, "Ngân sách", ProjectValidation.MaximumMoney);
        ProjectValidation.Dates(item.StartDate, item.Deadline, item.EndDate);

        // Mã bằng 0 nghĩa là thêm mới; mã lớn hơn 0 nghĩa là cập nhật.
        if (item.Id == 0)
            return await _dao.InsertAsync(item);

        await _dao.UpdateAsync(item);
        return item.Id;
    }

    public Task DeleteAsync(int id)
    {
        ProjectValidation.PositiveId(id, "bản ghi cần xóa");
        return _dao.DeleteAsync(id);
    }

    public List<ProjectDto> Search(ArrayList source, string keyword)
    {
        // Tìm trên ArrayList trong BUS theo yêu cầu môn học.
        keyword = keyword.Trim();
        var result = new List<ProjectDto>();
        foreach (ProjectDto item in source)
        {
            if (item.Id.ToString().Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                item.Status.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                item.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                item.DepartmentName.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                item.ManagerName.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                result.Add(item);
        }
        return result;
    }

    /// <summary>Tìm kiếm nâng cao trên ArrayList với các điều kiện AND, OR, NOT và toán tử ngày/số.</summary>
    public List<ProjectDto> SearchAdvanced(ArrayList source, IReadOnlyList<ProjectSearchCondition> conditions)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (conditions is null || conditions.Count == 0)
            return source.Cast<ProjectDto>().ToList();

        // Kiểm tra định dạng toàn bộ điều kiện trước khi lọc.
        var parsed_conditions = new List<Func<ProjectDto, bool>>(conditions.Count);
        foreach (ProjectSearchCondition condition in conditions)
            parsed_conditions.Add(BuildConditionPredicate(condition));

        var result = new List<ProjectDto>();
        foreach (object? obj in source)
        {
            if (obj is not ProjectDto item)
                continue;

            bool matched = true;
            for (int i = 0; i < conditions.Count; i++)
            {
                bool current = parsed_conditions[i](item);
                string logic = (conditions[i].LogicalOperator ?? "AND").Trim().ToUpperInvariant();
                if (i == 0)
                {
                    matched = logic == "NOT" ? !current : current;
                }
                else
                {
                    matched = logic switch
                    {
                        "OR" => matched || current,
                        "NOT" => matched && !current,
                        _ => matched && current
                    };
                }
            }

            if (matched)
                result.Add(item);
        }
        return result;
    }

    private static Func<ProjectDto, bool> BuildConditionPredicate(ProjectSearchCondition condition)
    {
        string field = (condition.FieldName ?? string.Empty).Trim();
        string op = (condition.ComparisonOperator ?? "=").Trim().ToUpperInvariant();
        string raw_value = (condition.Value ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(raw_value))
            throw new ArgumentException($"Giá trị tìm kiếm cho trường '{field}' không được để trống.");

        if (field is "Budget")
        {
            string normalized = raw_value.Replace(",", string.Empty).Replace(" ", string.Empty);
            if (!decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal target_money))
                throw new ArgumentException($"Giá trị '{raw_value}' không phải số hợp lệ cho trường Ngân sách.");

            ValidateComparisonOperator(op, is_range_field: true, field);
            return item => CompareDecimal(item.Budget, target_money, op);
        }

        if (field is "StartDate" or "Deadline" or "EndDate")
        {
            if (!DateTime.TryParseExact(raw_value, _date_formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime target_date) &&
                !DateTime.TryParse(raw_value, CultureInfo.GetCultureInfo("vi-VN"), DateTimeStyles.None, out target_date))
                throw new ArgumentException($"Giá trị '{raw_value}' không đúng định dạng ngày (dd/MM/yyyy hoặc yyyy-MM-dd).");

            ValidateComparisonOperator(op, is_range_field: true, field);
            return item =>
            {
                DateTime? actual = field switch
                {
                    "StartDate" => item.StartDate.Date,
                    "Deadline" => item.Deadline.Date,
                    _ => item.EndDate?.Date
                };
                if (!actual.HasValue)
                    return op == "<>";
                return CompareDate(actual.Value, target_date.Date, op);
            };
        }

        ValidateComparisonOperator(op, is_range_field: false, field);
        return item =>
        {
            string actual_text = field switch
            {
                "Name" => item.Name,
                "Status" => item.Status,
                "DepartmentName" => item.DepartmentName,
                "ManagerName" => item.ManagerName,
                "Description" => item.Description,
                _ => throw new ArgumentException($"Trường tìm kiếm '{field}' không được hỗ trợ.")
            };
            return CompareText(actual_text, raw_value, op);
        };
    }

    private static void ValidateComparisonOperator(string op, bool is_range_field, string field)
    {
        if (is_range_field)
        {
            if (op is not ("=" or "<>" or ">" or ">=" or "<" or "<="))
                throw new ArgumentException($"Toán tử '{op}' không hợp lệ cho trường '{field}'. Dùng =, <>, >, >=, < hoặc <=.");
            return;
        }

        if (op is not ("=" or "<>" or "LIKE"))
            throw new ArgumentException($"Toán tử '{op}' không hợp lệ cho trường chữ '{field}'. Dùng LIKE, = hoặc <>.");
    }

    private static bool CompareDecimal(decimal actual, decimal target, string op) => op switch
    {
        "=" => actual == target,
        "<>" => actual != target,
        ">" => actual > target,
        ">=" => actual >= target,
        "<" => actual < target,
        "<=" => actual <= target,
        _ => false
    };

    private static bool CompareDate(DateTime actual, DateTime target, string op) => op switch
    {
        "=" => actual == target,
        "<>" => actual != target,
        ">" => actual > target,
        ">=" => actual >= target,
        "<" => actual < target,
        "<=" => actual <= target,
        _ => false
    };

    private static bool CompareText(string actual, string target, string op) => op switch
    {
        "=" => actual.Equals(target, StringComparison.OrdinalIgnoreCase),
        "<>" => !actual.Equals(target, StringComparison.OrdinalIgnoreCase),
        _ => actual.Contains(target, StringComparison.OrdinalIgnoreCase)
    };
}

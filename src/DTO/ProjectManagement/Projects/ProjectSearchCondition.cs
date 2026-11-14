namespace DTO.ProjectManagement.Projects;

/// <summary>Một điều kiện lọc nâng cao cho Dự án (hỗ trợ AND, OR, NOT và toán tử ngày/số).</summary>
public sealed class ProjectSearchCondition
{
    public string LogicalOperator { get; set; } = "AND";
    public string FieldName { get; set; } = "Name";
    public string ComparisonOperator { get; set; } = "LIKE";
    public string Value { get; set; } = string.Empty;
}

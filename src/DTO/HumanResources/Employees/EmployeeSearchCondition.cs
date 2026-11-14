namespace DTO.HumanResources.Employees;

/// <summary>Một điều kiện lọc trong tìm kiếm nâng cao Nhân viên (hỗ trợ AND, OR, NOT).</summary>
public sealed class EmployeeSearchCondition
{
    public string LogicalOperator { get; set; } = "AND";
    public string FieldName { get; set; } = "FullName";
    public string ComparisonOperator { get; set; } = "LIKE";
    public string Value { get; set; } = string.Empty;
}

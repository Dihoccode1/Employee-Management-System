namespace DTO.HumanResources.Positions;

/// <summary>Thông tin chức vụ theo ERD (bao gồm mô tả và mức phụ cấp chức vụ).</summary>
public sealed class PositionDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal AllowanceAmount { get; set; }
}

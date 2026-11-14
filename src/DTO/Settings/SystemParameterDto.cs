using System;

namespace DTO.Settings;

/// <summary>Tham số nghiệp vụ dùng chung toàn hệ thống (số ngày công chuẩn, lương cơ bản tối thiểu, ...).</summary>
public sealed class SystemParameterDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string ValueType { get; set; } = "DECIMAL";
    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}

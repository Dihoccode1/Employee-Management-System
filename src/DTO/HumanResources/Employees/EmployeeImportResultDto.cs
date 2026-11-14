using System.Collections.Generic;

namespace DTO.HumanResources.Employees;

/// <summary>Kết quả kiểm tra và nhập dữ liệu nhân viên từ tệp Excel (.xls).</summary>
public sealed class EmployeeImportResultDto
{
    public int TotalRows { get; set; }
    public int ImportedCount { get; set; }
    public List<string> Errors { get; } = [];
    public bool HasErrors => Errors.Count > 0;
}

using System;

namespace DTO.HumanResources.Contracts;

/// <summary>Hợp đồng lao động theo ERD (giữ liên kết EmployeeId và EmployeeCode).</summary>
public sealed class ContractDto
{
    public int Id { get; set; }
    public string ContractCode { get; set; } = string.Empty;
    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string ContractType { get; set; } = "Xác định thời hạn";
    public DateTime StartDate { get; set; } = DateTime.Today;
    public DateTime? EndDate { get; set; }
    public decimal BaseSalary { get; set; }
    public string Status { get; set; } = "Còn hiệu lực";
}

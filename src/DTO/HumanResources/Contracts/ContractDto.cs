using System;
namespace DTO.HumanResources.Contracts;
public sealed class ContractDto {
    public string ContractCode { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; } = DateTime.Today;
    public DateTime? EndDate { get; set; }
    public decimal BaseSalary { get; set; }
}

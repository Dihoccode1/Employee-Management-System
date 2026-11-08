namespace DTO.HumanResources.Contracts;
public sealed class ContractDto {
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string ContractCode { get; set; } = string.Empty;
    public DateTime StartDate { get; set; } = DateTime.Today;
    public DateTime? EndDate { get; set; }
    public decimal BaseSalary { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
}

using System;

namespace HRM.DTO
{
    public class LaborContractDTO
    {
        public string contractId { get; set; }
        public string employeeId { get; set; }
        public string contractType { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public decimal baseSalary { get; set; }
        public string status { get; set; }

        public LaborContractDTO() { }

        public LaborContractDTO(string contractId, string employeeId, string contractType,
            DateTime startDate, DateTime endDate, decimal baseSalary, string status)
        {
            this.contractId = contractId;
            this.employeeId = employeeId;
            this.contractType = contractType;
            this.startDate = startDate;
            this.endDate = endDate;
            this.baseSalary = baseSalary;
            this.status = status;
        }
    }
}

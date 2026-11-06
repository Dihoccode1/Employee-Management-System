using System;

namespace HRM.DTO.HumanResources
{
    public class EmployeeDTO
    {
        public string employeeId { get; set; }
        public string departmentId { get; set; }
        public string positionId { get; set; }
        public string lastName { get; set; }
        public string firstName { get; set; }
        public DateTime dateOfBirth { get; set; }
        public string gender { get; set; }
        public string phoneNumber { get; set; }
        public string email { get; set; }
        public DateTime hireDate { get; set; }
        public string status { get; set; }
        public decimal currentBaseSalary { get; set; }
        public string degree { get; set; }

        public EmployeeDTO() { }

        public EmployeeDTO(string employeeId, string departmentId, string positionId,
            string lastName, string firstName, DateTime dateOfBirth, string gender,
            string phoneNumber, string email, DateTime hireDate, string status,
            decimal currentBaseSalary, string degree)
        {
            this.employeeId = employeeId;
            this.departmentId = departmentId;
            this.positionId = positionId;
            this.lastName = lastName;
            this.firstName = firstName;
            this.dateOfBirth = dateOfBirth;
            this.gender = gender;
            this.phoneNumber = phoneNumber;
            this.email = email;
            this.hireDate = hireDate;
            this.status = status;
            this.currentBaseSalary = currentBaseSalary;
            this.degree = degree;
        }
    }
}

using System.Collections.Generic;
using HRM.DAO;
using HRM.DTO;

namespace HRM.BUS
{
    public class EmployeeBUS
    {
        private readonly EmployeeDAO employeeDAO = new EmployeeDAO();

        public List<EmployeeDTO> getAllEmployees()
        {
            return employeeDAO.getAllEmployees();
        }

        public bool addEmployee(EmployeeDTO emp)
        {
            if (string.IsNullOrWhiteSpace(emp.employeeId) || string.IsNullOrWhiteSpace(emp.firstName) || emp.currentBaseSalary < 0)
                return false;
            return employeeDAO.insertEmployee(emp);
        }

        public bool updateEmployee(EmployeeDTO emp)
        {
            if (string.IsNullOrWhiteSpace(emp.employeeId) || string.IsNullOrWhiteSpace(emp.firstName) || emp.currentBaseSalary < 0)
                return false;
            return employeeDAO.updateEmployee(emp);
        }

        public bool deleteEmployee(string employeeId)
        {
            if (string.IsNullOrWhiteSpace(employeeId))
                return false;
            return employeeDAO.deleteEmployee(employeeId);
        }
    }
}

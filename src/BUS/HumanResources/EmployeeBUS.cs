using System.Collections.Generic;
using HRM.DAO.HumanResources;
using HRM.DTO.HumanResources;

namespace HRM.BUS.HumanResources
{
    internal class EmployeeBUS
    {
        public static List<EmployeeDTO> employeeList;

        public EmployeeBUS() { }

        public void resetEmployeeList()
        {
            employeeList = new List<EmployeeDTO>();
        }

        public List<EmployeeDTO> readEmployeeList()
        {
            EmployeeDAO dao = new EmployeeDAO();
            if (employeeList == null || employeeList.Count == 0)
            {
                employeeList = new List<EmployeeDTO>();
                employeeList = dao.readEmployeeList();
            }
            return employeeList;
        }

        public void add(EmployeeDTO emp)
        {
            if (string.IsNullOrWhiteSpace(emp.employeeId) || string.IsNullOrWhiteSpace(emp.firstName))
                return;
            if (searchById(emp.employeeId) != null)
                return;

            EmployeeDAO dao = new EmployeeDAO();
            dao.add(emp);
            if (employeeList == null) employeeList = new List<EmployeeDTO>();
            employeeList.Add(emp);
        }

        public void update(EmployeeDTO emp)
        {
            EmployeeDAO dao = new EmployeeDAO();
            dao.update(emp);
            if (employeeList != null)
            {
                for (int i = 0; i < employeeList.Count; i++)
                {
                    if (employeeList[i].employeeId.CompareTo(emp.employeeId) == 0)
                    {
                        employeeList[i] = emp;
                        break;
                    }
                }
            }
        }

        public void delete(string employeeId)
        {
            EmployeeDAO dao = new EmployeeDAO();
            dao.delete(employeeId);
            EmployeeDTO existing = searchById(employeeId);
            if (existing != null && employeeList != null)
            {
                employeeList.Remove(existing);
            }
        }

        public EmployeeDTO searchById(string id)
        {
            if (employeeList == null) readEmployeeList();
            foreach (EmployeeDTO emp in employeeList)
            {
                if (emp.employeeId.CompareTo(id) == 0)
                {
                    return emp;
                }
            }
            return null;
        }

        public List<EmployeeDTO> searchByLastName(string lastName)
        {
            if (employeeList == null) readEmployeeList();
            List<EmployeeDTO> result = new List<EmployeeDTO>();
            foreach (EmployeeDTO emp in employeeList)
            {
                if (emp.lastName.IndexOf(lastName) >= 0)
                {
                    result.Add(emp);
                }
            }
            return result;
        }
    }
}

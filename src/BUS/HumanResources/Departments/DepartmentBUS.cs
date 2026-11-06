using System.Collections.Generic;
using HRM.DAO;
using HRM.DTO;

namespace HRM.BUS
{
    public class DepartmentBUS
    {
        private readonly DepartmentDAO departmentDAO = new DepartmentDAO();

        public List<DepartmentDTO> getAllDepartments()
        {
            return departmentDAO.getAllDepartments();
        }

        public bool addDepartment(DepartmentDTO department)
        {
            if (string.IsNullOrWhiteSpace(department.departmentId) || string.IsNullOrWhiteSpace(department.departmentName))
                return false;
            return departmentDAO.insertDepartment(department);
        }

        public bool updateDepartment(DepartmentDTO department)
        {
            if (string.IsNullOrWhiteSpace(department.departmentId) || string.IsNullOrWhiteSpace(department.departmentName))
                return false;
            return departmentDAO.updateDepartment(department);
        }

        public bool deleteDepartment(string departmentId)
        {
            if (string.IsNullOrWhiteSpace(departmentId))
                return false;
            return departmentDAO.deleteDepartment(departmentId);
        }
    }
}

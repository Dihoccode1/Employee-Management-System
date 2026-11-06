using System.Collections.Generic;
using HRM.DAO.HumanResources;
using HRM.DTO.HumanResources;

namespace HRM.BUS.HumanResources
{
    internal class DepartmentBUS
    {
        public static List<DepartmentDTO> departmentList;

        public DepartmentBUS() { }

        public void resetDepartmentList()
        {
            departmentList = new List<DepartmentDTO>();
        }

        public List<DepartmentDTO> readDepartmentList()
        {
            DepartmentDAO dao = new DepartmentDAO();
            if (departmentList == null || departmentList.Count == 0)
            {
                departmentList = new List<DepartmentDTO>();
                departmentList = dao.readDepartmentList();
            }
            return departmentList;
        }

        public void add(DepartmentDTO dept)
        {
            if (string.IsNullOrWhiteSpace(dept.departmentId) || string.IsNullOrWhiteSpace(dept.departmentName))
                return;
            if (searchById(dept.departmentId) != null)
                return;

            DepartmentDAO dao = new DepartmentDAO();
            dao.add(dept);
            if (departmentList == null) departmentList = new List<DepartmentDTO>();
            departmentList.Add(dept);
        }

        public void update(DepartmentDTO dept)
        {
            DepartmentDAO dao = new DepartmentDAO();
            dao.update(dept);
            if (departmentList != null)
            {
                for (int i = 0; i < departmentList.Count; i++)
                {
                    if (departmentList[i].departmentId.CompareTo(dept.departmentId) == 0)
                    {
                        departmentList[i] = dept;
                        break;
                    }
                }
            }
        }

        public void delete(string departmentId)
        {
            DepartmentDAO dao = new DepartmentDAO();
            dao.delete(departmentId);
            DepartmentDTO existing = searchById(departmentId);
            if (existing != null && departmentList != null)
            {
                departmentList.Remove(existing);
            }
        }

        public DepartmentDTO searchById(string id)
        {
            if (departmentList == null) readDepartmentList();
            foreach (DepartmentDTO dept in departmentList)
            {
                if (dept.departmentId.CompareTo(id) == 0)
                {
                    return dept;
                }
            }
            return null;
        }

        public List<DepartmentDTO> searchByName(string name)
        {
            if (departmentList == null) readDepartmentList();
            List<DepartmentDTO> result = new List<DepartmentDTO>();
            foreach (DepartmentDTO dept in departmentList)
            {
                if (dept.departmentName.IndexOf(name) >= 0)
                {
                    result.Add(dept);
                }
            }
            return result;
        }
    }
}

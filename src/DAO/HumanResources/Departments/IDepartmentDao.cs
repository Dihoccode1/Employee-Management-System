using System.Collections.Generic;
using System.Threading.Tasks;
using DTO.HumanResources.Departments;
namespace DAO.HumanResources.Departments;
public interface IDepartmentDao {
    Task<List<DepartmentDto>> GetAllAsync();
    Task<int> InsertAsync(DepartmentDto item);
    Task UpdateAsync(DepartmentDto item);
    Task DeleteAsync(int id);
}

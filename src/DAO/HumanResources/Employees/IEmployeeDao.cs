using System.Collections.Generic;
using System.Threading.Tasks;
using DTO.HumanResources.Employees;
namespace DAO.HumanResources.Employees;
public interface IEmployeeDao {
    Task<List<EmployeeDto>> GetAllAsync();
    Task<int> InsertAsync(EmployeeDto item);
    Task UpdateAsync(EmployeeDto item);
    Task DeleteAsync(int id);
}

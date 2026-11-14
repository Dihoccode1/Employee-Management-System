using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DTO.HumanResources.Employees;

namespace DAO.HumanResources.Employees;

public interface IEmployeeDao
{
    Task<List<EmployeeDto>> GetAllAsync(CancellationToken cancellation_token = default);
    Task<bool> ExistsAsync(string employee_code, CancellationToken cancellation_token = default);
    Task<int> InsertAsync(EmployeeDto item, CancellationToken cancellation_token = default);
    Task UpdateAsync(EmployeeDto item, CancellationToken cancellation_token = default);
    Task DeleteAsync(string employee_code, CancellationToken cancellation_token = default);
}

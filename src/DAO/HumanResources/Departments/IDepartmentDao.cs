using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DTO.HumanResources.Departments;

namespace DAO.HumanResources.Departments;

public interface IDepartmentDao
{
    Task<List<DepartmentDto>> GetAllAsync(CancellationToken cancellation_token = default);
    Task<bool> ExistsAsync(string code, CancellationToken cancellation_token = default);
    Task<int> InsertAsync(DepartmentDto item, CancellationToken cancellation_token = default);
    Task UpdateAsync(DepartmentDto item, CancellationToken cancellation_token = default);
    Task DeleteAsync(string code, CancellationToken cancellation_token = default);
}

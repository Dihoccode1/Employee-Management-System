using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DAO.Infrastructure;
using DTO.HumanResources.Employees;

namespace DAO.HumanResources.Employees;

/// <summary>Repository đọc danh sách nhân viên phục vụ Web API, dùng chung logic truy vấn với EmployeeDao.</summary>
public sealed class EmployeeRepository(MyConnection factory) : IEmployeeRepository
{
    public async Task<IReadOnlyList<EmployeeDto>> GetAllAsync(CancellationToken cancellation_token = default)
    {
        var employee_dao = new EmployeeDao(factory);
        return await employee_dao.GetAllAsync(cancellation_token);
    }
}

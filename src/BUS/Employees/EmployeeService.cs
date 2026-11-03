using DTO.Employees;
using DAO.Employees;
namespace BUS.Employees;
public sealed class EmployeeService(IEmployeeRepository repository)
{
    public Task<IReadOnlyList<EmployeeDto>> GetAllAsync(CancellationToken cancellationToken = default)
        => repository.GetAllAsync(cancellationToken);
}
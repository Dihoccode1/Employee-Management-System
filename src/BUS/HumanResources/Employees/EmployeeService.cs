using DTO.HumanResources.Employees;
using DAO.HumanResources.Employees;
namespace BUS.HumanResources.Employees;
public sealed class EmployeeService(IEmployeeRepository repository)
{
    public Task<IReadOnlyList<EmployeeDto>> GetAllAsync(CancellationToken cancellationToken = default)
        => repository.GetAllAsync(cancellationToken);
}
using DTO.HumanResources.Employees;
namespace DAO.HumanResources.Employees;
public interface IEmployeeRepository
{
    Task<IReadOnlyList<EmployeeDto>> GetAllAsync(CancellationToken cancellationToken = default);
}
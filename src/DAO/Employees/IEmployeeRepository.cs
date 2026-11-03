using DTO.Employees;
namespace DAO.Employees;
public interface IEmployeeRepository
{
    Task<IReadOnlyList<EmployeeDto>> GetAllAsync(CancellationToken cancellationToken = default);
}
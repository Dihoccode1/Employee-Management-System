using DTO.Employees;
namespace DAL.Employees;
public interface IEmployeeRepository
{
    Task<IReadOnlyList<EmployeeDto>> GetAllAsync(CancellationToken cancellationToken = default);
}
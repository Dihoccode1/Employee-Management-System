using DTO.Employees;
using DAO.Infrastructure;
namespace DAO.Employees;
public sealed class EmployeeRepository(MyConnection factory) : IEmployeeRepository
{
    public async Task<IReadOnlyList<EmployeeDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var result = new List<EmployeeDto>();
        await using var connection = factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, EmployeeCode, FullName, Email, DepartmentId, PositionId, HireDate, IsActive FROM dbo.Employees ORDER BY EmployeeCode";
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            result.Add(new EmployeeDto {
                Id = reader.GetInt32(0), EmployeeCode = reader.GetString(1),
                FullName = reader.GetString(2), Email = reader.IsDBNull(3) ? null : reader.GetString(3),
                DepartmentId = reader.IsDBNull(4) ? null : reader.GetInt32(4),
                PositionId = reader.IsDBNull(5) ? null : reader.GetInt32(5),
                HireDate = reader.GetDateTime(6), IsActive = reader.GetBoolean(7)
            });
        return result;
    }
}
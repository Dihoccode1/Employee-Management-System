using DAO.Infrastructure;
using DTO.ProjectManagement;

namespace DAO.ProjectManagement;

public sealed class ProjectLookupDao
{
    private readonly MyConnection _connectionFactory;

    public ProjectLookupDao(MyConnection connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public Task<List<LookupDto>> GetDepartmentsAsync()
    {
        return ReadAsync("SELECT Id, Code + N' - ' + Name AS Name FROM dbo.Departments ORDER BY Name;");
    }

    public Task<List<LookupDto>> GetEmployeesAsync()
    {
        // Giữ cả nhân viên đã nghỉ để vẫn xem/sửa được dữ liệu lịch sử.
        return ReadAsync("SELECT Id, EmployeeCode + N' - ' + FullName AS Name FROM dbo.Employees ORDER BY FullName;");
    }

    private async Task<List<LookupDto>> ReadAsync(string query)
    {
        var items = new List<LookupDto>();
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        // query chỉ nhận các câu SQL cố định ở hai hàm phía trên.
        command.CommandText = query;
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            items.Add(new LookupDto { Id = reader.GetInt32(0), Name = reader.GetString(1) });
        return items;
    }
}

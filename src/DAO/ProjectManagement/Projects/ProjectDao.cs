using System.Data;
using Microsoft.Data.SqlClient;
using DAO.Infrastructure;
using DTO.ProjectManagement.Projects;

namespace DAO.ProjectManagement.Projects;

public sealed class ProjectDao : IProjectDao
{
    private readonly MyConnection _connectionFactory;

    public ProjectDao(MyConnection connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<List<ProjectDto>> GetAllAsync()
    {
        var items = new List<ProjectDto>();
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT p.Id,
                   p.Name,
                   p.StartDate,
                   p.EndDate,
                   p.Deadline,
                   p.Description,
                   p.Budget,
                   p.DepartmentId,
                   p.ManagerId,
                   p.Status,
                   d.Code AS DepartmentCode,
                   d.Name AS DepartmentName,
                   e.EmployeeCode AS ManagerCode,
                   e.FullName AS ManagerName
            FROM dbo.Projects p
            JOIN dbo.Departments d ON d.Id = p.DepartmentId
            JOIN dbo.Employees e ON e.Id = p.ManagerId
            ORDER BY p.Id DESC;
            """;
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            items.Add(new ProjectDto
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
                EndDate = reader.IsDBNull(reader.GetOrdinal("EndDate")) ? null : reader.GetDateTime(reader.GetOrdinal("EndDate")),
                Deadline = reader.GetDateTime(reader.GetOrdinal("Deadline")),
                Description = reader.GetString(reader.GetOrdinal("Description")),
                Budget = reader.GetDecimal(reader.GetOrdinal("Budget")),
                DepartmentId = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                ManagerId = reader.GetInt32(reader.GetOrdinal("ManagerId")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                DepartmentCode = reader.GetString(reader.GetOrdinal("DepartmentCode")),
                DepartmentName = reader.GetString(reader.GetOrdinal("DepartmentName")),
                ManagerCode = reader.GetString(reader.GetOrdinal("ManagerCode")),
                ManagerName = reader.GetString(reader.GetOrdinal("ManagerName")),
            });
        }
        return items;
    }

    public async Task<int> InsertAsync(ProjectDto item)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO dbo.Projects
                (Name,
                 StartDate,
                 EndDate,
                 Deadline,
                 Description,
                 Budget,
                 DepartmentId,
                 ManagerId,
                 Status)
            OUTPUT INSERTED.Id
            VALUES
                (@Name,
                 @StartDate,
                 @EndDate,
                 @Deadline,
                 @Description,
                 @Budget,
                 @DepartmentId,
                 @ManagerId,
                 @Status);
            """;
        AddParameters(command, item);
        try
        {
            return Convert.ToInt32(await command.ExecuteScalarAsync());
        }
        catch (SqlException exception)
        {
            throw SqlParameterHelper.CreateWriteException(exception);
        }
    }

    public async Task UpdateAsync(ProjectDto item)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE dbo.Projects
            SET Name = @Name,
                StartDate = @StartDate,
                EndDate = @EndDate,
                Deadline = @Deadline,
                Description = @Description,
                Budget = @Budget,
                DepartmentId = @DepartmentId,
                ManagerId = @ManagerId,
                Status = @Status
            WHERE Id = @Id;
            """;
        command.Parameters.Add("@Id", SqlDbType.Int).Value = item.Id;
        AddParameters(command, item);
        try
        {
            if (await command.ExecuteNonQueryAsync() == 0)
                throw new InvalidOperationException("Bản ghi không còn tồn tại. Vui lòng tải lại.");
        }
        catch (SqlException exception)
        {
            throw SqlParameterHelper.CreateWriteException(exception);
        }
    }

    public async Task DeleteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM dbo.Projects WHERE Id = @Id;";
        command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
        try
        {
            // Khóa ngoại trong SQL Server chặn xóa khi còn dữ liệu con.
            if (await command.ExecuteNonQueryAsync() == 0)
                throw new InvalidOperationException("Bản ghi không còn tồn tại. Vui lòng tải lại.");
        }
        catch (SqlException exception) when (exception.Number == 547)
        {
            throw new InvalidOperationException("Không thể xóa vì còn dữ liệu liên quan. Hãy xóa dữ liệu con trước.", exception);
        }
    }

    private static void AddParameters(SqlCommand command, ProjectDto item)
    {
        // Dùng tham số để dữ liệu nhập không trở thành một phần của câu SQL.
        command.Parameters.Add("@Name", SqlDbType.NVarChar, 200).Value = item.Name;
        command.Parameters.Add("@StartDate", SqlDbType.Date).Value = item.StartDate.Date;
        command.Parameters.Add("@EndDate", SqlDbType.Date).Value = (object?)item.EndDate?.Date ?? DBNull.Value;
        command.Parameters.Add("@Deadline", SqlDbType.Date).Value = item.Deadline.Date;
        command.Parameters.Add("@Description", SqlDbType.NVarChar, 2000).Value = item.Description;
        SqlParameterHelper.AddDecimal(command, "@Budget", item.Budget, 18);
        command.Parameters.Add("@DepartmentId", SqlDbType.Int).Value = item.DepartmentId;
        command.Parameters.Add("@ManagerId", SqlDbType.Int).Value = item.ManagerId;
        command.Parameters.Add("@Status", SqlDbType.NVarChar, 50).Value = item.Status;
    }
}

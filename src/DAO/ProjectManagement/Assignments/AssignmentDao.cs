using System.Data;
using Microsoft.Data.SqlClient;
using DAO.Infrastructure;
using DTO.ProjectManagement.Assignments;

namespace DAO.ProjectManagement.Assignments;

public sealed class AssignmentDao : IAssignmentDao
{
    private readonly MyConnection _connectionFactory;

    public AssignmentDao(MyConnection connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<List<AssignmentDto>> GetAllAsync()
    {
        var items = new List<AssignmentDto>();
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT a.Id,
                   a.TaskId,
                   a.EmployeeId,
                   a.AssignedDate,
                   a.StartDate,
                   a.Deadline,
                   a.CompletedDate,
                   a.CompletionPercentage,
                   a.Status,
                   a.Notes,
                   t.Name AS TaskName,
                   e.FullName AS EmployeeName,
                   p.Name AS ProjectName
            FROM dbo.Assignments a
            JOIN dbo.ProjectTasks t ON t.Id = a.TaskId
            JOIN dbo.ProjectDetails d ON d.Id = t.ProjectDetailId
            JOIN dbo.Projects p ON p.Id = d.ProjectId
            JOIN dbo.Employees e ON e.Id = a.EmployeeId
            ORDER BY a.Id DESC;
            """;
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            items.Add(new AssignmentDto
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                TaskId = reader.GetInt32(reader.GetOrdinal("TaskId")),
                EmployeeId = reader.GetInt32(reader.GetOrdinal("EmployeeId")),
                AssignedDate = reader.GetDateTime(reader.GetOrdinal("AssignedDate")),
                StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
                Deadline = reader.GetDateTime(reader.GetOrdinal("Deadline")),
                CompletedDate = reader.IsDBNull(reader.GetOrdinal("CompletedDate")) ? null : reader.GetDateTime(reader.GetOrdinal("CompletedDate")),
                CompletionPercentage = reader.GetDecimal(reader.GetOrdinal("CompletionPercentage")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                Notes = reader.GetString(reader.GetOrdinal("Notes")),
                TaskName = reader.GetString(reader.GetOrdinal("TaskName")),
                EmployeeName = reader.GetString(reader.GetOrdinal("EmployeeName")),
                ProjectName = reader.GetString(reader.GetOrdinal("ProjectName")),
            });
        }
        return items;
    }

    public async Task<int> InsertAsync(AssignmentDto item)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO dbo.Assignments
                (TaskId,
                 EmployeeId,
                 AssignedDate,
                 StartDate,
                 Deadline,
                 CompletedDate,
                 CompletionPercentage,
                 Status,
                 Notes)
            OUTPUT INSERTED.Id
            VALUES
                (@TaskId,
                 @EmployeeId,
                 @AssignedDate,
                 @StartDate,
                 @Deadline,
                 @CompletedDate,
                 @CompletionPercentage,
                 @Status,
                 @Notes);
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

    public async Task UpdateAsync(AssignmentDto item)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE dbo.Assignments
            SET TaskId = @TaskId,
                EmployeeId = @EmployeeId,
                AssignedDate = @AssignedDate,
                StartDate = @StartDate,
                Deadline = @Deadline,
                CompletedDate = @CompletedDate,
                CompletionPercentage = @CompletionPercentage,
                Status = @Status,
                Notes = @Notes
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
        command.CommandText = "DELETE FROM dbo.Assignments WHERE Id = @Id;";
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

    private static void AddParameters(SqlCommand command, AssignmentDto item)
    {
        // Dùng tham số để dữ liệu nhập không trở thành một phần của câu SQL.
        command.Parameters.Add("@TaskId", SqlDbType.Int).Value = item.TaskId;
        command.Parameters.Add("@EmployeeId", SqlDbType.Int).Value = item.EmployeeId;
        command.Parameters.Add("@AssignedDate", SqlDbType.Date).Value = item.AssignedDate.Date;
        command.Parameters.Add("@StartDate", SqlDbType.Date).Value = item.StartDate.Date;
        command.Parameters.Add("@Deadline", SqlDbType.Date).Value = item.Deadline.Date;
        command.Parameters.Add("@CompletedDate", SqlDbType.Date).Value = (object?)item.CompletedDate?.Date ?? DBNull.Value;
        SqlParameterHelper.AddDecimal(command, "@CompletionPercentage", item.CompletionPercentage, 5);
        command.Parameters.Add("@Status", SqlDbType.NVarChar, 50).Value = item.Status;
        command.Parameters.Add("@Notes", SqlDbType.NVarChar, 2000).Value = item.Notes;
    }
}

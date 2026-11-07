using System.Data;
using Microsoft.Data.SqlClient;
using DAO.Infrastructure;
using DTO.ProjectManagement.Tasks;

namespace DAO.ProjectManagement.Tasks;

public sealed class ProjectTaskDao : IProjectTaskDao
{
    private readonly MyConnection _connectionFactory;

    public ProjectTaskDao(MyConnection connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<List<ProjectTaskDto>> GetAllAsync()
    {
        var items = new List<ProjectTaskDto>();
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT t.Id,
                   t.ProjectDetailId,
                   t.Name,
                   t.Description,
                   t.Type,
                   t.Priority,
                   t.StartDate,
                   t.Deadline,
                   t.CompletedDate,
                   t.EstimatedHours,
                   t.CompletionPercentage,
                   t.Status,
                   t.Notes,
                   p.Name AS ProjectName
            FROM dbo.ProjectTasks t
            JOIN dbo.ProjectDetails d ON d.Id = t.ProjectDetailId
            JOIN dbo.Projects p ON p.Id = d.ProjectId
            ORDER BY t.Id DESC;
            """;
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            items.Add(new ProjectTaskDto
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                ProjectDetailId = reader.GetInt32(reader.GetOrdinal("ProjectDetailId")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Description = reader.GetString(reader.GetOrdinal("Description")),
                Type = reader.GetString(reader.GetOrdinal("Type")),
                Priority = reader.GetString(reader.GetOrdinal("Priority")),
                StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
                Deadline = reader.GetDateTime(reader.GetOrdinal("Deadline")),
                CompletedDate = reader.IsDBNull(reader.GetOrdinal("CompletedDate")) ? null : reader.GetDateTime(reader.GetOrdinal("CompletedDate")),
                EstimatedHours = reader.GetDecimal(reader.GetOrdinal("EstimatedHours")),
                CompletionPercentage = reader.GetDecimal(reader.GetOrdinal("CompletionPercentage")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                Notes = reader.GetString(reader.GetOrdinal("Notes")),
                ProjectName = reader.GetString(reader.GetOrdinal("ProjectName")),
            });
        }
        return items;
    }

    public async Task<int> InsertAsync(ProjectTaskDto item)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO dbo.ProjectTasks
                (ProjectDetailId,
                 Name,
                 Description,
                 Type,
                 Priority,
                 StartDate,
                 Deadline,
                 CompletedDate,
                 EstimatedHours,
                 CompletionPercentage,
                 Status,
                 Notes)
            OUTPUT INSERTED.Id
            VALUES
                (@ProjectDetailId,
                 @Name,
                 @Description,
                 @Type,
                 @Priority,
                 @StartDate,
                 @Deadline,
                 @CompletedDate,
                 @EstimatedHours,
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

    public async Task UpdateAsync(ProjectTaskDto item)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE dbo.ProjectTasks
            SET ProjectDetailId = @ProjectDetailId,
                Name = @Name,
                Description = @Description,
                Type = @Type,
                Priority = @Priority,
                StartDate = @StartDate,
                Deadline = @Deadline,
                CompletedDate = @CompletedDate,
                EstimatedHours = @EstimatedHours,
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
        command.CommandText = "DELETE FROM dbo.ProjectTasks WHERE Id = @Id;";
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

    private static void AddParameters(SqlCommand command, ProjectTaskDto item)
    {
        // Dùng tham số để dữ liệu nhập không trở thành một phần của câu SQL.
        command.Parameters.Add("@ProjectDetailId", SqlDbType.Int).Value = item.ProjectDetailId;
        command.Parameters.Add("@Name", SqlDbType.NVarChar, 200).Value = item.Name;
        command.Parameters.Add("@Description", SqlDbType.NVarChar, 2000).Value = item.Description;
        command.Parameters.Add("@Type", SqlDbType.NVarChar, 100).Value = item.Type;
        command.Parameters.Add("@Priority", SqlDbType.NVarChar, 50).Value = item.Priority;
        command.Parameters.Add("@StartDate", SqlDbType.Date).Value = item.StartDate.Date;
        command.Parameters.Add("@Deadline", SqlDbType.Date).Value = item.Deadline.Date;
        command.Parameters.Add("@CompletedDate", SqlDbType.Date).Value = (object?)item.CompletedDate?.Date ?? DBNull.Value;
        SqlParameterHelper.AddDecimal(command, "@EstimatedHours", item.EstimatedHours, 10);
        SqlParameterHelper.AddDecimal(command, "@CompletionPercentage", item.CompletionPercentage, 5);
        command.Parameters.Add("@Status", SqlDbType.NVarChar, 50).Value = item.Status;
        command.Parameters.Add("@Notes", SqlDbType.NVarChar, 2000).Value = item.Notes;
    }
}

using System.Data;
using Microsoft.Data.SqlClient;
using DAO.Infrastructure;
using DTO.ProjectManagement.ProjectDetails;

namespace DAO.ProjectManagement.ProjectDetails;

public sealed class ProjectDetailDao : IProjectDetailDao
{
    private readonly MyConnection _connectionFactory;

    public ProjectDetailDao(MyConnection connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<List<ProjectDetailDto>> GetAllAsync()
    {
        var items = new List<ProjectDetailDto>();
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT d.Id,
                   d.ProjectId,
                   d.CompletionPercentage,
                   d.Status,
                   d.Allowance,
                   d.Bonus,
                   p.Name AS ProjectName
            FROM dbo.ProjectDetails d
            JOIN dbo.Projects p ON p.Id = d.ProjectId
            ORDER BY d.Id DESC;
            """;
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            items.Add(new ProjectDetailDto
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                ProjectId = reader.GetInt32(reader.GetOrdinal("ProjectId")),
                CompletionPercentage = reader.GetDecimal(reader.GetOrdinal("CompletionPercentage")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                Allowance = reader.GetDecimal(reader.GetOrdinal("Allowance")),
                Bonus = reader.GetDecimal(reader.GetOrdinal("Bonus")),
                ProjectName = reader.GetString(reader.GetOrdinal("ProjectName")),
            });
        }
        return items;
    }

    public async Task<int> InsertAsync(ProjectDetailDto item)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO dbo.ProjectDetails
                (ProjectId,
                 CompletionPercentage,
                 Status,
                 Allowance,
                 Bonus)
            OUTPUT INSERTED.Id
            VALUES
                (@ProjectId,
                 @CompletionPercentage,
                 @Status,
                 @Allowance,
                 @Bonus);
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

    public async Task UpdateAsync(ProjectDetailDto item)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE dbo.ProjectDetails
            SET ProjectId = @ProjectId,
                CompletionPercentage = @CompletionPercentage,
                Status = @Status,
                Allowance = @Allowance,
                Bonus = @Bonus
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
        command.CommandText = "DELETE FROM dbo.ProjectDetails WHERE Id = @Id;";
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

    private static void AddParameters(SqlCommand command, ProjectDetailDto item)
    {
        // Dùng tham số để dữ liệu nhập không trở thành một phần của câu SQL.
        command.Parameters.Add("@ProjectId", SqlDbType.Int).Value = item.ProjectId;
        SqlParameterHelper.AddDecimal(command, "@CompletionPercentage", item.CompletionPercentage, 5);
        command.Parameters.Add("@Status", SqlDbType.NVarChar, 50).Value = item.Status;
        SqlParameterHelper.AddDecimal(command, "@Allowance", item.Allowance, 18);
        SqlParameterHelper.AddDecimal(command, "@Bonus", item.Bonus, 18);
    }
}

using System.Data;
using DAO.Infrastructure;
using DTO.ProjectManagement.ProjectDetails;
using Microsoft.Data.SqlClient;

namespace DAO.ProjectManagement.ProjectDetails;

public class ProjectDetailDao
{
    private readonly MyConnection connectionFactory;

    public ProjectDetailDao(MyConnection connectionFactory)
    {
        this.connectionFactory = connectionFactory;
    }

    // DAO reads and writes SQL Server data; GUI displays messages.
    public List<ProjectDetailDto> getAll()
    {
        List<ProjectDetailDto> items = new List<ProjectDetailDto>();
        using SqlConnection connection = connectionFactory.CreateConnection();
        connection.Open();
        using SqlCommand command = connection.CreateCommand();
        command.CommandText = "SELECT id, projectId, completionRate, status, allowance, reward FROM dbo.ProjectDetails ORDER BY id";
        using SqlDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            ProjectDetailDto item = new ProjectDetailDto
            {
                id = reader.GetInt32(0),
                projectId = reader.GetInt32(1),
                completionRate = reader.GetDecimal(2),
                status = reader.GetString(3),
                allowance = reader.GetDecimal(4),
                reward = reader.GetDecimal(5)
            };
            items.Add(item);
        }
        return items;
    }

    public bool exists(int id)
    {
        using SqlConnection connection = connectionFactory.CreateConnection();
        connection.Open();
        using SqlCommand command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM dbo.ProjectDetails WHERE id = @id";
        command.Parameters.Add("@id", SqlDbType.Int).Value = id;
        return (int)command.ExecuteScalar()! > 0;
    }

    public int add(ProjectDetailDto item)
    {
        using SqlConnection connection = connectionFactory.CreateConnection();
        connection.Open();
        using SqlCommand command = connection.CreateCommand();
        command.CommandText = @"INSERT INTO dbo.ProjectDetails
            (projectId, completionRate, status, allowance, reward)
            VALUES (@projectId, @completionRate, @status, @allowance, @reward);
            SELECT CONVERT(INT, SCOPE_IDENTITY());";
        addParameters(command, item);
        try
        {
            return (int)command.ExecuteScalar()!;
        }
        catch (SqlException exception)
        {
            throw ProjectDatabaseErrors.createWriteError(exception);
        }
    }

    public void update(ProjectDetailDto item)
    {
        using SqlConnection connection = connectionFactory.CreateConnection();
        connection.Open();
        using SqlCommand command = connection.CreateCommand();
        command.CommandText = @"UPDATE dbo.ProjectDetails SET
            projectId = @projectId, completionRate = @completionRate, status = @status, allowance = @allowance, reward = @reward
            WHERE id = @id";
        command.Parameters.Add("@id", SqlDbType.Int).Value = item.id;
        addParameters(command, item);
        try
        {
            if (command.ExecuteNonQuery() == 0)
                throw new InvalidOperationException("Bản ghi không còn tồn tại. Hãy tải lại danh sách.");
        }
        catch (SqlException exception)
        {
            throw ProjectDatabaseErrors.createWriteError(exception);
        }
    }

    public void delete(int id)
    {
        using SqlConnection connection = connectionFactory.CreateConnection();
        connection.Open();
        using SqlCommand command = connection.CreateCommand();
        command.CommandText = "DELETE FROM dbo.ProjectDetails WHERE id = @id";
        command.Parameters.Add("@id", SqlDbType.Int).Value = id;
        try
        {
            if (command.ExecuteNonQuery() == 0)
                throw new InvalidOperationException("Bản ghi không còn tồn tại. Hãy tải lại danh sách.");
        }
        catch (SqlException exception)
        {
            throw ProjectDatabaseErrors.createWriteError(exception);
        }
    }

    public bool projectHasDetail(int projectId, int excludedId)
    {
        using SqlConnection connection = connectionFactory.CreateConnection();
        connection.Open();
        using SqlCommand command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM dbo.ProjectDetails WHERE projectId = @projectId AND id <> @id";
        command.Parameters.Add("@projectId", SqlDbType.Int).Value = projectId;
        command.Parameters.Add("@id", SqlDbType.Int).Value = excludedId;
        return (int)command.ExecuteScalar()! > 0;
    }

    // Always pass input through SQL parameters.
    private void addParameters(SqlCommand command, ProjectDetailDto item)
    {
        command.Parameters.Add("@projectId", SqlDbType.Int).Value = item.projectId;
        command.Parameters.Add("@completionRate", SqlDbType.Decimal).Value = item.completionRate;
        command.Parameters["@completionRate"].Precision = 5;
        command.Parameters["@completionRate"].Scale = 2;
        command.Parameters.Add("@status", SqlDbType.NVarChar, 30).Value = item.status;
        command.Parameters.Add("@allowance", SqlDbType.Decimal).Value = item.allowance;
        command.Parameters["@allowance"].Precision = 18;
        command.Parameters["@allowance"].Scale = 2;
        command.Parameters.Add("@reward", SqlDbType.Decimal).Value = item.reward;
        command.Parameters["@reward"].Precision = 18;
        command.Parameters["@reward"].Scale = 2;
    }
}

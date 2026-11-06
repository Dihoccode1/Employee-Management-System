using System.Data;
using DAO.Infrastructure;
using DTO.ProjectManagement.Projects;
using Microsoft.Data.SqlClient;

namespace DAO.ProjectManagement.Projects;

public class ProjectDao
{
    private readonly MyConnection connectionFactory;

    public ProjectDao(MyConnection connectionFactory)
    {
        this.connectionFactory = connectionFactory;
    }

    // DAO reads and writes SQL Server data; GUI displays messages.
    public List<ProjectDto> getAll()
    {
        List<ProjectDto> items = new List<ProjectDto>();
        using SqlConnection connection = connectionFactory.CreateConnection();
        connection.Open();
        using SqlCommand command = connection.CreateCommand();
        command.CommandText = "SELECT id, name, startDate, endDate, deadline, description, budget, departmentId, managerId, status FROM dbo.Projects ORDER BY id";
        using SqlDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            ProjectDto item = new ProjectDto
            {
                id = reader.GetInt32(0),
                name = reader.GetString(1),
                startDate = reader.GetDateTime(2),
                endDate = reader.IsDBNull(3) ? null : reader.GetDateTime(3),
                deadline = reader.GetDateTime(4),
                description = reader.GetString(5),
                budget = reader.GetDecimal(6),
                departmentId = reader.GetInt32(7),
                managerId = reader.GetInt32(8),
                status = reader.GetString(9)
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
        command.CommandText = "SELECT COUNT(*) FROM dbo.Projects WHERE id = @id";
        command.Parameters.Add("@id", SqlDbType.Int).Value = id;
        return (int)command.ExecuteScalar()! > 0;
    }

    public int add(ProjectDto item)
    {
        using SqlConnection connection = connectionFactory.CreateConnection();
        connection.Open();
        using SqlCommand command = connection.CreateCommand();
        command.CommandText = @"INSERT INTO dbo.Projects
            (name, startDate, endDate, deadline, description, budget, departmentId, managerId, status)
            VALUES (@name, @startDate, @endDate, @deadline, @description, @budget, @departmentId, @managerId, @status);
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

    public void update(ProjectDto item)
    {
        using SqlConnection connection = connectionFactory.CreateConnection();
        connection.Open();
        using SqlCommand command = connection.CreateCommand();
        command.CommandText = @"UPDATE dbo.Projects SET
            name = @name, startDate = @startDate, endDate = @endDate, deadline = @deadline, description = @description, budget = @budget, departmentId = @departmentId, managerId = @managerId, status = @status
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
        command.CommandText = "DELETE FROM dbo.Projects WHERE id = @id";
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

    // Always pass input through SQL parameters.
    private void addParameters(SqlCommand command, ProjectDto item)
    {
        command.Parameters.Add("@name", SqlDbType.NVarChar, 150).Value = item.name;
        command.Parameters.Add("@startDate", SqlDbType.Date).Value = item.startDate.Date;
        command.Parameters.Add("@endDate", SqlDbType.Date).Value = (object?)item.endDate?.Date ?? DBNull.Value;
        command.Parameters.Add("@deadline", SqlDbType.Date).Value = item.deadline.Date;
        command.Parameters.Add("@description", SqlDbType.NVarChar, 1000).Value = item.description;
        command.Parameters.Add("@budget", SqlDbType.Decimal).Value = item.budget;
        command.Parameters["@budget"].Precision = 18;
        command.Parameters["@budget"].Scale = 2;
        command.Parameters.Add("@departmentId", SqlDbType.Int).Value = item.departmentId;
        command.Parameters.Add("@managerId", SqlDbType.Int).Value = item.managerId;
        command.Parameters.Add("@status", SqlDbType.NVarChar, 30).Value = item.status;
    }
}

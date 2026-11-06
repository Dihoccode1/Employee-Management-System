using System.Data;
using DAO.Infrastructure;
using DTO.ProjectManagement.Tasks;
using Microsoft.Data.SqlClient;

namespace DAO.ProjectManagement.Tasks;

public class ProjectTaskDao
{
    private readonly MyConnection connectionFactory;

    public ProjectTaskDao(MyConnection connectionFactory)
    {
        this.connectionFactory = connectionFactory;
    }

    // DAO reads and writes SQL Server data; GUI displays messages.
    public List<ProjectTaskDto> getAll()
    {
        List<ProjectTaskDto> items = new List<ProjectTaskDto>();
        using SqlConnection connection = connectionFactory.CreateConnection();
        connection.Open();
        using SqlCommand command = connection.CreateCommand();
        command.CommandText = "SELECT id, projectDetailId, name, description, taskType, priority, startDate, deadline, completedDate, estimatedHours, completionRate, status, notes FROM dbo.ProjectTasks ORDER BY id";
        using SqlDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            ProjectTaskDto item = new ProjectTaskDto
            {
                id = reader.GetInt32(0),
                projectDetailId = reader.GetInt32(1),
                name = reader.GetString(2),
                description = reader.GetString(3),
                taskType = reader.GetString(4),
                priority = reader.GetString(5),
                startDate = reader.GetDateTime(6),
                deadline = reader.GetDateTime(7),
                completedDate = reader.IsDBNull(8) ? null : reader.GetDateTime(8),
                estimatedHours = reader.GetDecimal(9),
                completionRate = reader.GetDecimal(10),
                status = reader.GetString(11),
                notes = reader.GetString(12)
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
        command.CommandText = "SELECT COUNT(*) FROM dbo.ProjectTasks WHERE id = @id";
        command.Parameters.Add("@id", SqlDbType.Int).Value = id;
        return (int)command.ExecuteScalar()! > 0;
    }

    public int add(ProjectTaskDto item)
    {
        using SqlConnection connection = connectionFactory.CreateConnection();
        connection.Open();
        using SqlCommand command = connection.CreateCommand();
        command.CommandText = @"INSERT INTO dbo.ProjectTasks
            (projectDetailId, name, description, taskType, priority, startDate, deadline, completedDate, estimatedHours, completionRate, status, notes)
            VALUES (@projectDetailId, @name, @description, @taskType, @priority, @startDate, @deadline, @completedDate, @estimatedHours, @completionRate, @status, @notes);
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

    public void update(ProjectTaskDto item)
    {
        using SqlConnection connection = connectionFactory.CreateConnection();
        connection.Open();
        using SqlCommand command = connection.CreateCommand();
        command.CommandText = @"UPDATE dbo.ProjectTasks SET
            projectDetailId = @projectDetailId, name = @name, description = @description, taskType = @taskType, priority = @priority, startDate = @startDate, deadline = @deadline, completedDate = @completedDate, estimatedHours = @estimatedHours, completionRate = @completionRate, status = @status, notes = @notes
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
        command.CommandText = "DELETE FROM dbo.ProjectTasks WHERE id = @id";
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

    public int getProjectId(int taskId)
    {
        using SqlConnection connection = connectionFactory.CreateConnection();
        connection.Open();
        using SqlCommand command = connection.CreateCommand();
        command.CommandText = @"SELECT d.projectId FROM dbo.ProjectTasks t
                                JOIN dbo.ProjectDetails d ON d.id = t.projectDetailId
                                WHERE t.id = @id";
        command.Parameters.Add("@id", SqlDbType.Int).Value = taskId;
        object? result = command.ExecuteScalar();
        return result == null ? 0 : Convert.ToInt32(result);
    }

    // Always pass input through SQL parameters.
    private void addParameters(SqlCommand command, ProjectTaskDto item)
    {
        command.Parameters.Add("@projectDetailId", SqlDbType.Int).Value = item.projectDetailId;
        command.Parameters.Add("@name", SqlDbType.NVarChar, 150).Value = item.name;
        command.Parameters.Add("@description", SqlDbType.NVarChar, 1000).Value = item.description;
        command.Parameters.Add("@taskType", SqlDbType.NVarChar, 100).Value = item.taskType;
        command.Parameters.Add("@priority", SqlDbType.NVarChar, 20).Value = item.priority;
        command.Parameters.Add("@startDate", SqlDbType.Date).Value = item.startDate.Date;
        command.Parameters.Add("@deadline", SqlDbType.Date).Value = item.deadline.Date;
        command.Parameters.Add("@completedDate", SqlDbType.Date).Value = (object?)item.completedDate?.Date ?? DBNull.Value;
        command.Parameters.Add("@estimatedHours", SqlDbType.Decimal).Value = item.estimatedHours;
        command.Parameters["@estimatedHours"].Precision = 10;
        command.Parameters["@estimatedHours"].Scale = 2;
        command.Parameters.Add("@completionRate", SqlDbType.Decimal).Value = item.completionRate;
        command.Parameters["@completionRate"].Precision = 5;
        command.Parameters["@completionRate"].Scale = 2;
        command.Parameters.Add("@status", SqlDbType.NVarChar, 30).Value = item.status;
        command.Parameters.Add("@notes", SqlDbType.NVarChar, 1000).Value = item.notes;
    }
}

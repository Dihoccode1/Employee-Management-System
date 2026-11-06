using System.Data;
using DAO.Infrastructure;
using DTO.ProjectManagement.Assignments;
using Microsoft.Data.SqlClient;

namespace DAO.ProjectManagement.Assignments;

public class AssignmentDao
{
    private readonly MyConnection connectionFactory;

    public AssignmentDao(MyConnection connectionFactory)
    {
        this.connectionFactory = connectionFactory;
    }

    // DAO reads and writes SQL Server data; GUI displays messages.
    public List<AssignmentDto> getAll()
    {
        List<AssignmentDto> items = new List<AssignmentDto>();
        using SqlConnection connection = connectionFactory.CreateConnection();
        connection.Open();
        using SqlCommand command = connection.CreateCommand();
        command.CommandText = "SELECT id, taskId, projectId, employeeId, assignedDate, startDate, deadline, completedDate, completionRate, contributionScore, status, notes FROM dbo.Assignments ORDER BY id";
        using SqlDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            AssignmentDto item = new AssignmentDto
            {
                id = reader.GetInt32(0),
                taskId = reader.GetInt32(1),
                projectId = reader.GetInt32(2),
                employeeId = reader.GetInt32(3),
                assignedDate = reader.GetDateTime(4),
                startDate = reader.GetDateTime(5),
                deadline = reader.GetDateTime(6),
                completedDate = reader.IsDBNull(7) ? null : reader.GetDateTime(7),
                completionRate = reader.GetDecimal(8),
                contributionScore = reader.GetDecimal(9),
                status = reader.GetString(10),
                notes = reader.GetString(11)
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
        command.CommandText = "SELECT COUNT(*) FROM dbo.Assignments WHERE id = @id";
        command.Parameters.Add("@id", SqlDbType.Int).Value = id;
        return (int)command.ExecuteScalar()! > 0;
    }

    public int add(AssignmentDto item)
    {
        using SqlConnection connection = connectionFactory.CreateConnection();
        connection.Open();
        using SqlCommand command = connection.CreateCommand();
        command.CommandText = @"INSERT INTO dbo.Assignments
            (taskId, projectId, employeeId, assignedDate, startDate, deadline, completedDate, completionRate, contributionScore, status, notes)
            VALUES (@taskId, @projectId, @employeeId, @assignedDate, @startDate, @deadline, @completedDate, @completionRate, @contributionScore, @status, @notes);
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

    public void update(AssignmentDto item)
    {
        using SqlConnection connection = connectionFactory.CreateConnection();
        connection.Open();
        using SqlCommand command = connection.CreateCommand();
        command.CommandText = @"UPDATE dbo.Assignments SET
            taskId = @taskId, projectId = @projectId, employeeId = @employeeId, assignedDate = @assignedDate, startDate = @startDate, deadline = @deadline, completedDate = @completedDate, completionRate = @completionRate, contributionScore = @contributionScore, status = @status, notes = @notes
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
        command.CommandText = "DELETE FROM dbo.Assignments WHERE id = @id";
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

    public bool assignmentExists(int taskId, int employeeId, int excludedId)
    {
        using SqlConnection connection = connectionFactory.CreateConnection();
        connection.Open();
        using SqlCommand command = connection.CreateCommand();
        command.CommandText = @"SELECT COUNT(*) FROM dbo.Assignments
                                WHERE taskId = @taskId AND employeeId = @employeeId AND id <> @id";
        command.Parameters.Add("@taskId", SqlDbType.Int).Value = taskId;
        command.Parameters.Add("@employeeId", SqlDbType.Int).Value = employeeId;
        command.Parameters.Add("@id", SqlDbType.Int).Value = excludedId;
        return (int)command.ExecuteScalar()! > 0;
    }

    // Always pass input through SQL parameters.
    private void addParameters(SqlCommand command, AssignmentDto item)
    {
        command.Parameters.Add("@taskId", SqlDbType.Int).Value = item.taskId;
        command.Parameters.Add("@projectId", SqlDbType.Int).Value = item.projectId;
        command.Parameters.Add("@employeeId", SqlDbType.Int).Value = item.employeeId;
        command.Parameters.Add("@assignedDate", SqlDbType.Date).Value = item.assignedDate.Date;
        command.Parameters.Add("@startDate", SqlDbType.Date).Value = item.startDate.Date;
        command.Parameters.Add("@deadline", SqlDbType.Date).Value = item.deadline.Date;
        command.Parameters.Add("@completedDate", SqlDbType.Date).Value = (object?)item.completedDate?.Date ?? DBNull.Value;
        command.Parameters.Add("@completionRate", SqlDbType.Decimal).Value = item.completionRate;
        command.Parameters["@completionRate"].Precision = 5;
        command.Parameters["@completionRate"].Scale = 2;
        command.Parameters.Add("@contributionScore", SqlDbType.Decimal).Value = item.contributionScore;
        command.Parameters["@contributionScore"].Precision = 10;
        command.Parameters["@contributionScore"].Scale = 2;
        command.Parameters.Add("@status", SqlDbType.NVarChar, 30).Value = item.status;
        command.Parameters.Add("@notes", SqlDbType.NVarChar, 1000).Value = item.notes;
    }
}

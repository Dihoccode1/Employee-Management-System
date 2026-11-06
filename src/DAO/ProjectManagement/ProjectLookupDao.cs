using System.Data;
using DAO.Infrastructure;
using DTO.ProjectManagement;
using Microsoft.Data.SqlClient;

namespace DAO.ProjectManagement;

public class ProjectLookupDao
{
    private readonly MyConnection connectionFactory;

    public ProjectLookupDao(MyConnection connectionFactory)
    {
        this.connectionFactory = connectionFactory;
    }

    public List<LookupItemDto> getDepartments()
    {
        List<LookupItemDto> items = new List<LookupItemDto>();
        using SqlConnection connection = connectionFactory.CreateConnection();
        connection.Open();
        using SqlCommand command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Code + N' - ' + Name FROM dbo.Departments ORDER BY Code";
        using SqlDataReader reader = command.ExecuteReader();
        while (reader.Read())
            items.Add(new LookupItemDto { id = reader.GetInt32(0), name = reader.GetString(1) });
        return items;
    }

    public List<LookupItemDto> getEmployees()
    {
        List<LookupItemDto> items = new List<LookupItemDto>();
        using SqlConnection connection = connectionFactory.CreateConnection();
        connection.Open();
        using SqlCommand command = connection.CreateCommand();
        // Include inactive employees so historical assignments remain editable.
        command.CommandText = "SELECT Id, EmployeeCode + N' - ' + FullName FROM dbo.Employees ORDER BY EmployeeCode";
        using SqlDataReader reader = command.ExecuteReader();
        while (reader.Read())
            items.Add(new LookupItemDto { id = reader.GetInt32(0), name = reader.GetString(1) });
        return items;
    }

    public bool departmentExists(int id)
    {
        using SqlConnection connection = connectionFactory.CreateConnection();
        connection.Open();
        using SqlCommand command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM dbo.Departments WHERE Id = @id";
        command.Parameters.Add("@id", SqlDbType.Int).Value = id;
        return (int)command.ExecuteScalar()! > 0;
    }

    public bool employeeExists(int id)
    {
        using SqlConnection connection = connectionFactory.CreateConnection();
        connection.Open();
        using SqlCommand command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM dbo.Employees WHERE Id = @id";
        command.Parameters.Add("@id", SqlDbType.Int).Value = id;
        return (int)command.ExecuteScalar()! > 0;
    }
}

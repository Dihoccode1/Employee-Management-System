using Microsoft.Data.SqlClient;
namespace DAO.Infrastructure;
public sealed class MyConnection(string connectionString)
{
    public SqlConnection CreateConnection() => new(connectionString);
}
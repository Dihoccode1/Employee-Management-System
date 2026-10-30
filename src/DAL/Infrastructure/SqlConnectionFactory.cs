using Microsoft.Data.SqlClient;
namespace DAL.Infrastructure;
public sealed class SqlConnectionFactory(string connectionString)
{
    public SqlConnection CreateConnection() => new(connectionString);
}
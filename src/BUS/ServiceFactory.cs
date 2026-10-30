using BUS.Employees;
using DAL.Employees;
using DAL.Infrastructure;
namespace BUS;
public static class ServiceFactory
{
    public static EmployeeService CreateEmployeeService(string connectionString)
        => new(new EmployeeRepository(new SqlConnectionFactory(connectionString)));
}
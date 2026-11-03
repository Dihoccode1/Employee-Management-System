using BUS.Employees;
using DAO.Employees;
using DAO.Infrastructure;
namespace BUS;
public static class ServiceFactory
{
    public static EmployeeService CreateEmployeeService(string connectionString)
        => new(new EmployeeRepository(new MyConnection(connectionString)));
}
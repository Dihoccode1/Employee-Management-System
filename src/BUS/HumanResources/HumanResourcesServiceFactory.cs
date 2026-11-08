using BUS.HumanResources.Contracts;
using BUS.HumanResources.Departments;
using BUS.HumanResources.Employees;
using BUS.HumanResources.Positions;
using DAO.HumanResources.Contracts;
using DAO.HumanResources.Departments;
using DAO.HumanResources.Employees;
using DAO.HumanResources.Positions;
using DAO.Infrastructure;
namespace BUS.HumanResources;
public static class HumanResourcesServiceFactory {
    public static DepartmentService CreateDepartmentService(string conn) => new(new DepartmentDao(new MyConnection(conn)));
    public static PositionService CreatePositionService(string conn) => new(new PositionDao(new MyConnection(conn)));
    public static EmployeeService CreateEmployeeService(string conn) => new(new EmployeeDao(new MyConnection(conn)));
    public static ContractService CreateContractService(string conn) => new(new ContractDao(new MyConnection(conn)));
}

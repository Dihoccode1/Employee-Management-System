using BUS.HumanResources.Contracts;
using BUS.HumanResources.Departments;
using BUS.HumanResources.Employees;
using BUS.HumanResources.Positions;
using BUS.ImportExport;
using BUS.Settings;
using DAO.HumanResources.Contracts;
using DAO.HumanResources.Departments;
using DAO.HumanResources.Employees;
using DAO.HumanResources.Positions;
using DAO.Infrastructure;
using DAO.Settings;

namespace BUS.HumanResources;

public static class HumanResourcesServiceFactory
{
    public static DepartmentService CreateDepartmentService(string connection_string)
        => new(new DepartmentDao(new MyConnection(connection_string)));

    public static PositionService CreatePositionService(string connection_string)
        => new(new PositionDao(new MyConnection(connection_string)));

    public static EmployeeService CreateEmployeeService(string connection_string)
    {
        var connection = new MyConnection(connection_string);
        return new EmployeeService(new EmployeeDao(connection), new SystemParameterDao(connection));
    }

    public static ContractService CreateContractService(string connection_string)
    {
        var connection = new MyConnection(connection_string);
        return new ContractService(new ContractDao(connection), new SystemParameterDao(connection));
    }

    public static EmployeeExcelService CreateEmployeeExcelService(string connection_string)
        => new(
            CreateEmployeeService(connection_string),
            CreateDepartmentService(connection_string),
            CreatePositionService(connection_string));

    public static SystemParameterService CreateSystemParameterService(string connection_string)
        => new(new SystemParameterDao(new MyConnection(connection_string)));
}

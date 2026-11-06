using BUS.HumanResources.Employees;
using BUS.ProjectManagement;
using BUS.ProjectManagement.Projects;
using BUS.ProjectManagement.ProjectDetails;
using BUS.ProjectManagement.Tasks;
using BUS.ProjectManagement.Assignments;
using DAO.HumanResources.Employees;
using DAO.Infrastructure;
using DAO.ProjectManagement;
using DAO.ProjectManagement.Projects;
using DAO.ProjectManagement.ProjectDetails;
using DAO.ProjectManagement.Tasks;
using DAO.ProjectManagement.Assignments;
namespace BUS;
public static class ServiceFactory
{

    public static ProjectBus createProjectBus(string connectionString)
    {
        MyConnection connection = new MyConnection(connectionString);
        return new ProjectBus(new ProjectDao(connection), new ProjectLookupDao(connection));
    }

    public static ProjectDetailBus createProjectDetailBus(string connectionString)
    {
        MyConnection connection = new MyConnection(connectionString);
        return new ProjectDetailBus(new ProjectDetailDao(connection), new ProjectDao(connection));
    }

    public static ProjectTaskBus createProjectTaskBus(string connectionString)
    {
        MyConnection connection = new MyConnection(connectionString);
        return new ProjectTaskBus(new ProjectTaskDao(connection), new ProjectDetailDao(connection));
    }

    public static AssignmentBus createAssignmentBus(string connectionString)
    {
        MyConnection connection = new MyConnection(connectionString);
        return new AssignmentBus(new AssignmentDao(connection), new ProjectTaskDao(connection),
            new ProjectDao(connection), new ProjectLookupDao(connection));
    }

    public static ProjectLookupBus createProjectLookupBus(string connectionString)
    {
        MyConnection connection = new MyConnection(connectionString);
        return new ProjectLookupBus(new ProjectLookupDao(connection));
    }
    public static EmployeeService CreateEmployeeService(string connectionString)
        => new(new EmployeeRepository(new MyConnection(connectionString)));
}

using BUS.HumanResources.Employees;
using DAO.HumanResources.Employees;
using DAO.Infrastructure;
using BUS.ProjectManagement;
using BUS.ProjectManagement.Projects;
using BUS.ProjectManagement.ProjectDetails;
using BUS.ProjectManagement.Tasks;
using BUS.ProjectManagement.Assignments;
using DAO.ProjectManagement;
using DAO.ProjectManagement.Projects;
using DAO.ProjectManagement.ProjectDetails;
using DAO.ProjectManagement.Tasks;
using DAO.ProjectManagement.Assignments;
namespace BUS;
public static class ServiceFactory
{
    public static EmployeeService CreateEmployeeService(string connectionString)
        => new(new EmployeeRepository(new MyConnection(connectionString)));

    // GUI chỉ gọi factory; việc tạo DAO được giữ trong BUS.
    public static ProjectService CreateProjectService(string connectionString)
    {
        var dao = new ProjectDao(new MyConnection(connectionString));
        return new ProjectService(dao);
    }

    public static ProjectDetailService CreateProjectDetailService(string connectionString)
    {
        var dao = new ProjectDetailDao(new MyConnection(connectionString));
        return new ProjectDetailService(dao);
    }

    public static ProjectTaskService CreateProjectTaskService(string connectionString)
    {
        var dao = new ProjectTaskDao(new MyConnection(connectionString));
        return new ProjectTaskService(dao);
    }

    public static AssignmentService CreateAssignmentService(string connectionString)
    {
        var dao = new AssignmentDao(new MyConnection(connectionString));
        return new AssignmentService(dao);
    }

    public static ProjectLookupService CreateProjectLookupService(string connectionString)
    {
        var dao = new ProjectLookupDao(new MyConnection(connectionString));
        return new ProjectLookupService(dao);
    }
}

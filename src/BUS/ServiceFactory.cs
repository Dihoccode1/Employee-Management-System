using BUS.HumanResources;
using BUS.HumanResources.Employees;
using BUS.ImportExport;
using BUS.ProjectManagement;
using BUS.ProjectManagement.Assignments;
using BUS.ProjectManagement.ProjectDetails;
using BUS.ProjectManagement.Projects;
using BUS.ProjectManagement.Tasks;
using BUS.SalaryInformation.Attendance;
using BUS.Settings;
using DAO.Infrastructure;
using DAO.ProjectManagement;
using DAO.ProjectManagement.Assignments;
using DAO.ProjectManagement.ProjectDetails;
using DAO.ProjectManagement.Projects;
using DAO.ProjectManagement.Tasks;
using DAO.SalaryInformation.Attendance;

namespace BUS;

public static class ServiceFactory
{
    public static EmployeeService CreateEmployeeService(string connection_string)
        => HumanResourcesServiceFactory.CreateEmployeeService(connection_string);

    public static EmployeeExcelService CreateEmployeeExcelService(string connection_string)
        => HumanResourcesServiceFactory.CreateEmployeeExcelService(connection_string);

    public static SystemParameterService CreateSystemParameterService(string connection_string)
        => HumanResourcesServiceFactory.CreateSystemParameterService(connection_string);

    public static AttendanceService CreateAttendanceService(string connection_string)
        => new(new AttendanceDao(new MyConnection(connection_string)));

    // GUI chỉ gọi factory; việc tạo DAO được giữ trong BUS.
    public static ProjectService CreateProjectService(string connection_string)
    {
        var dao = new ProjectDao(new MyConnection(connection_string));
        return new ProjectService(dao);
    }

    public static ProjectDetailService CreateProjectDetailService(string connection_string)
    {
        var dao = new ProjectDetailDao(new MyConnection(connection_string));
        return new ProjectDetailService(dao);
    }

    public static ProjectTaskService CreateProjectTaskService(string connection_string)
    {
        var dao = new ProjectTaskDao(new MyConnection(connection_string));
        return new ProjectTaskService(dao);
    }

    public static AssignmentService CreateAssignmentService(string connection_string)
    {
        var dao = new AssignmentDao(new MyConnection(connection_string));
        return new AssignmentService(dao);
    }

    public static ProjectLookupService CreateProjectLookupService(string connection_string)
    {
        var dao = new ProjectLookupDao(new MyConnection(connection_string));
        return new ProjectLookupService(dao);
    }
}

using System.Text.RegularExpressions;
using BUS;
using DTO.ProjectManagement.Projects;
using DTO.ProjectManagement.ProjectDetails;
using DTO.ProjectManagement.Tasks;
using DTO.ProjectManagement.Assignments;
using Microsoft.Data.SqlClient;

namespace ProjectManagement.Tests;

internal static class SqlIntegration
{
    public static async Task RunAsync()
    {
        // Chỉ kiểm thử trên LocalDB riêng, không dùng HRM_CONNECTION_STRING của ứng dụng.
        string databaseName = "EMS_ProjectTests_" + Guid.NewGuid().ToString("N");
        string masterConnection = @"Server=(localdb)\CodexEMSProjectTests;Database=master;Integrated Security=True;Encrypt=False;Connect Timeout=15";
        var builder = new SqlConnectionStringBuilder(masterConnection) { InitialCatalog = databaseName };
        string testConnection = builder.ConnectionString;
        bool created = false;
        bool succeeded = false;
        try
        {
            await ExecuteAsync(masterConnection, $"CREATE DATABASE [{databaseName}];");
            created = true;
            string root = FindRepositoryRoot();
            foreach (string file in new[] { "01_CreateDatabase.sql", "02_SeedData.sql",
                "03_ProjectManagement.sql", "04_ProjectManagementSeed.sql",
                "03_ProjectManagement.sql", "04_ProjectManagementSeed.sql" })
            {
                string script = await File.ReadAllTextAsync(Path.Combine(root, "database", file));
                script = script.Replace("HumanResourceManagement", databaseName);
                using var connection = new SqlConnection(testConnection);
                await connection.OpenAsync();
                foreach (string batch in Regex.Split(script, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase))
                {
                    if (string.IsNullOrWhiteSpace(batch)) continue;
                    using var command = new SqlCommand(batch, connection);
                    await command.ExecuteNonQueryAsync();
                }
            }
            await VerifyCrudAsync(testConnection);
            succeeded = true;
        }
        finally
        {
            if (created)
            {
                SqlConnection.ClearAllPools();
                if (succeeded && Environment.GetEnvironmentVariable("HRM_KEEP_TEST_DATABASE") == "1")
                {
                    // Giữ dữ liệu tạm khi cần mở GUI để kiểm tra trực quan.
                    Console.WriteLine($"UI test database: {databaseName}");
                    await File.WriteAllTextAsync(Path.Combine(AppContext.BaseDirectory, "test-database.txt"), databaseName);
                }
                else
                {
                    // Tên chỉ được sinh từ GUID phía trên, không nhận đầu vào người dùng.
                    await ExecuteAsync(masterConnection,
                        $"ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{databaseName}];");
                }
            }
        }
    }

    private static async Task VerifyCrudAsync(string connectionString)
    {
        var projects = ServiceFactory.CreateProjectService(connectionString);
        var details = ServiceFactory.CreateProjectDetailService(connectionString);
        var tasks = ServiceFactory.CreateProjectTaskService(connectionString);
        var assignments = ServiceFactory.CreateAssignmentService(connectionString);
        var lookups = ServiceFactory.CreateProjectLookupService(connectionString);
        int departmentId = (await lookups.GetDepartmentsAsync()).First().Id;
        int employeeId = (await lookups.GetEmployeesAsync()).First().Id;
        Program.Assert((await projects.GetAllAsync()).Count == 1, "Seed scripts must be repeatable.");
        var project = new ProjectDto
        {
            Name = "Dự án '); DROP TABLE dbo.Projects;--", Description = "Kiểm thử tiếng Việt",
            DepartmentId = departmentId, ManagerId = employeeId, Budget = 123456789.12m,
            StartDate = new DateTime(2026, 11, 1), Deadline = new DateTime(2026, 12, 1), Status = "Đang làm"
        };
        project.Id = await projects.SaveAsync(project);
        var savedProject = (await projects.GetAllAsync()).Single(x => x.Id == project.Id);
        Program.Assert(savedProject.Name == project.Name && savedProject.Budget == project.Budget &&
            savedProject.EndDate == null && savedProject.DepartmentName.Length > 0 &&
            savedProject.ManagerName.Length > 0, "Project mapping, Unicode and SQL parameters.");
        project.Description = "Đã sửa";
        project.EndDate = project.Deadline.AddDays(1);
        await projects.SaveAsync(project);
        savedProject = (await projects.GetAllAsync()).Single(x => x.Id == project.Id);
        Program.Assert(savedProject.Description == "Đã sửa" && savedProject.EndDate == project.EndDate, "Update project.");

        var detail = new ProjectDetailDto
        {
            ProjectId = project.Id, CompletionPercentage = 12.34m, Status = "Nhập tay",
            Allowance = 123456.78m, Bonus = 987654.32m
        };
        detail.Id = await details.SaveAsync(detail);
        detail.Bonus = 765432.10m;
        await details.SaveAsync(detail);
        var savedDetail = (await details.GetAllAsync()).Single(x => x.Id == detail.Id);
        Program.Assert(savedDetail.Allowance == detail.Allowance && savedDetail.Bonus == detail.Bonus &&
            savedDetail.ProjectName == project.Name, "Manual money and detail joins.");

        var task = new ProjectTaskDto
        {
            ProjectDetailId = detail.Id, Name = "Công việc A", Description = "Mô tả A",
            Type = "Lập trình", Priority = "Cao", EstimatedHours = 15.25m,
            StartDate = project.StartDate, Deadline = project.Deadline, Status = "Chờ", Notes = "Ghi chú",
            CompletionPercentage = 23.45m
        };
        task.Id = await tasks.SaveAsync(task);
        task.CompletedDate = task.Deadline.AddDays(1);
        task.Notes = "Ghi chú mới";
        await tasks.SaveAsync(task);
        var savedTask = (await tasks.GetAllAsync()).Single(x => x.Id == task.Id);
        Program.Assert(savedTask.CompletedDate == task.CompletedDate && savedTask.Notes == task.Notes &&
            savedTask.EstimatedHours == task.EstimatedHours && savedTask.ProjectName == project.Name, "Task mapping.");

        var assignment = new AssignmentDto
        {
            TaskId = task.Id, EmployeeId = employeeId, AssignedDate = project.StartDate,
            StartDate = project.StartDate, Deadline = project.Deadline,
            CompletionPercentage = 34.56m, Status = "Tạm dừng", Notes = "Không tự tính tiến độ"
        };
        assignment.Id = await assignments.SaveAsync(assignment);
        assignment.CompletedDate = assignment.Deadline.AddDays(3);
        await assignments.SaveAsync(assignment);
        var savedAssignment = (await assignments.GetAllAsync()).Single(x => x.Id == assignment.Id);
        Program.Assert(savedAssignment.CompletionPercentage == 34.56m && savedAssignment.Status == "Tạm dừng" &&
            savedAssignment.CompletedDate == assignment.CompletedDate && savedAssignment.TaskName == task.Name &&
            savedAssignment.ProjectName == project.Name && savedAssignment.EmployeeName.Length > 0, "Assignment mapping.");
        savedDetail = (await details.GetAllAsync()).Single(x => x.Id == detail.Id);
        savedTask = (await tasks.GetAllAsync()).Single(x => x.Id == task.Id);
        Program.Assert(savedDetail.CompletionPercentage == 12.34m && savedTask.CompletionPercentage == 23.45m &&
            savedDetail.Bonus == 765432.10m && savedDetail.Status == "Nhập tay", "Children must not recalculate parents.");

        await Program.Reject<InvalidOperationException>(() => projects.DeleteAsync(project.Id));
        await Program.Reject<InvalidOperationException>(() => details.DeleteAsync(detail.Id));
        await Program.Reject<InvalidOperationException>(() => tasks.DeleteAsync(task.Id));
        await assignments.DeleteAsync(assignment.Id);
        await Program.Reject<InvalidOperationException>(() => assignments.DeleteAsync(assignment.Id));
        await Program.Reject<InvalidOperationException>(() => assignments.SaveAsync(assignment));
        await tasks.DeleteAsync(task.Id);
        await details.DeleteAsync(detail.Id);
        await projects.DeleteAsync(project.Id);
        Program.Assert((await projects.GetAllAsync()).All(x => x.Id != project.Id), "Delete child to parent.");
        detail.Id = 0;
        await Program.Reject<InvalidOperationException>(() => details.SaveAsync(detail));
        await Program.Reject<SqlException>(() => ExecuteAsync(connectionString,
            $"UPDATE dbo.ProjectDetails SET CompletionPercentage = 101 WHERE Id = (SELECT MIN(Id) FROM dbo.ProjectDetails);"));
        await Program.Reject<SqlException>(() => ExecuteAsync(connectionString,
            $"UPDATE dbo.ProjectDetails SET Allowance = -1 WHERE Id = (SELECT MIN(Id) FROM dbo.ProjectDetails);"));
        await Program.Reject<SqlException>(() => ExecuteAsync(connectionString,
            $"UPDATE dbo.ProjectTasks SET Deadline = DATEADD(day, -1, StartDate);"));
    }

    private static async Task ExecuteAsync(string connectionString, string sql)
    {
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        using var command = new SqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory != null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "HumanResourceManagement.sln")))
                return directory.FullName;
            directory = directory.Parent;
        }
        throw new DirectoryNotFoundException("Không tìm thấy thư mục solution.");
    }
}

using System.Collections;
using BUS.ProjectManagement.Projects;
using BUS.ProjectManagement.ProjectDetails;
using BUS.ProjectManagement.Tasks;
using BUS.ProjectManagement.Assignments;
using DTO.ProjectManagement.Projects;
using DTO.ProjectManagement.ProjectDetails;
using DTO.ProjectManagement.Tasks;
using DTO.ProjectManagement.Assignments;

namespace ProjectManagement.Tests;

internal static class Program
{
    private static int _passed;

    private static async Task Main(string[] args)
    {
        await Check("Project: validation, insert and update", TestProjectsAsync);
        await Check("Detail: manual money and progress", TestDetailsAsync);
        await Check("Task: date, hours and percentage validation", TestTasksAsync);
        await Check("Assignment: employee and manual state", TestAssignmentsAsync);
        await Check("Search: ArrayList, empty term, case and no mutation", TestSearchAsync);
        if (args.Contains("--sql"))
            await Check("SQL: scripts, CRUD, foreign keys and manual values", SqlIntegration.RunAsync);
        Console.WriteLine($"Passed {_passed} test groups.");
    }

    private static async Task Check(string name, Func<Task> action)
    {
        await action();
        _passed++;
        Console.WriteLine($"PASS: {name}");
    }

    internal static void Assert(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }

    internal static async Task Reject<TException>(Func<Task> action) where TException : Exception
    {
        try { await action(); }
        catch (TException) { return; }
        throw new Exception($"Expected {typeof(TException).Name}.");
    }

    private static async Task TestProjectsAsync()
    {
        var dao = new FakeProjectDao();
        var service = new ProjectService(dao);
        ProjectDto Valid() => new()
        {
            Name = "  Dự án A  ", Status = " Đang làm ", DepartmentId = 1, ManagerId = 2,
            StartDate = new DateTime(2026, 11, 1), Deadline = new DateTime(2026, 11, 30)
        };
        var item = Valid();
        Assert(await service.SaveAsync(item) == 7, "Insert must return new identity.");
        Assert(item.Name == "Dự án A" && item.Status == "Đang làm", "Trim text.");
        item.Id = 5;
        Assert(await service.SaveAsync(item) == 5, "Update must preserve identity.");
        foreach (Action<ProjectDto> change in new Action<ProjectDto>[]
        {
            x => x.Name = " ", x => x.Name = new string('a', 201), x => x.Status = "",
            x => x.DepartmentId = 0, x => x.ManagerId = -1, x => x.Budget = -1,
            x => x.Budget = 0.001m, x => x.Budget = 10000000000000000m,
            x => x.Deadline = x.StartDate.AddDays(-1), x => x.EndDate = x.StartDate.AddDays(-1),
            x => x.Id = -1, x => x.Description = new string('a', 2001)
        })
        {
            item = Valid(); change(item);
            await Reject<ArgumentException>(() => service.SaveAsync(item));
        }
        Assert(dao.WriteCount == 2, "Invalid input must not reach DAO.");
        item = Valid(); item.EndDate = item.Deadline.AddDays(5);
        await service.SaveAsync(item);
        await Reject<ArgumentException>(() => service.DeleteAsync(0));
        await service.DeleteAsync(5);
        Assert(dao.WriteCount == 4, "Allow late completion and valid delete.");
    }

    private static async Task TestDetailsAsync()
    {
        var dao = new FakeProjectDetailDao();
        var service = new ProjectDetailService(dao);
        var item = new ProjectDetailDto
        {
            ProjectId = 1, Status = "Tùy nhập", CompletionPercentage = 13.25m,
            Allowance = 123456.78m, Bonus = 654321.09m
        };
        await service.SaveAsync(item);
        Assert(item.Allowance == 123456.78m && item.Bonus == 654321.09m &&
               item.CompletionPercentage == 13.25m && item.Status == "Tùy nhập", "Manual values must stay unchanged.");
        foreach (Action<ProjectDetailDto> change in new Action<ProjectDetailDto>[]
        {
            x => x.CompletionPercentage = 100.01m, x => x.CompletionPercentage = -1,
            x => x.Bonus = -1, x => x.Allowance = -1, x => x.Bonus = 1.001m,
            x => x.ProjectId = 0, x => x.Status = " "
        })
        {
            var invalid = new ProjectDetailDto { ProjectId = 1, Status = "A" };
            change(invalid);
            await Reject<ArgumentException>(() => service.SaveAsync(invalid));
        }
        Assert(dao.WriteCount == 1, "Invalid detail must not reach DAO.");
    }

    private static async Task TestTasksAsync()
    {
        var dao = new FakeProjectTaskDao();
        var service = new ProjectTaskService(dao);
        ProjectTaskDto Valid() => new()
        {
            Name = "Công việc", ProjectDetailId = 1, Status = "Tạm dừng",
            StartDate = new DateTime(2026, 11, 1), Deadline = new DateTime(2026, 11, 2),
            CompletedDate = new DateTime(2026, 11, 4), CompletionPercentage = 100, EstimatedHours = 1.25m
        };
        var item = Valid();
        await service.SaveAsync(item);
        foreach (Action<ProjectTaskDto> change in new Action<ProjectTaskDto>[]
        {
            x => x.ProjectDetailId = 0, x => x.Name = "", x => x.EstimatedHours = -1,
            x => x.EstimatedHours = 100000000m, x => x.CompletionPercentage = 101,
            x => x.CompletionPercentage = -0.01m, x => x.CompletedDate = x.StartDate.AddDays(-1),
            x => x.Deadline = x.StartDate.AddDays(-1), x => x.Priority = new string('a', 51)
        })
        {
            item = Valid(); change(item);
            await Reject<ArgumentException>(() => service.SaveAsync(item));
        }
        Assert(dao.WriteCount == 1, "Invalid task must not reach DAO.");
    }

    private static async Task TestAssignmentsAsync()
    {
        var dao = new FakeAssignmentDao();
        var service = new AssignmentService(dao);
        var item = new AssignmentDto
        {
            TaskId = 1, EmployeeId = 2, Status = "Chờ duyệt", CompletionPercentage = 45.5m,
            AssignedDate = new DateTime(2026, 11, 1),
            StartDate = new DateTime(2026, 11, 2), Deadline = new DateTime(2026, 11, 3)
        };
        await service.SaveAsync(item);
        Assert(item.CompletedDate == null && item.Status == "Chờ duyệt" &&
               item.CompletionPercentage == 45.5m, "No automatic date/status/progress changes.");
        item.EmployeeId = 0;
        await Reject<ArgumentException>(() => service.SaveAsync(item));
        item.EmployeeId = 2; item.TaskId = 0;
        await Reject<ArgumentException>(() => service.SaveAsync(item));
        item.TaskId = 1; item.CompletionPercentage = 101;
        await Reject<ArgumentException>(() => service.SaveAsync(item));
        Assert(dao.WriteCount == 1, "Invalid assignment must not reach DAO.");
    }

    private static Task TestSearchAsync()
    {
        var projects = new ArrayList
        {
            new ProjectDto { Id = 1, Name = "Alpha", DepartmentName = "Kỹ thuật", ManagerName = "An", Status = "Doing" },
            new ProjectDto { Id = 2, Name = "Beta", Status = "Done" }
        };
        var projectService = new ProjectService(new FakeProjectDao());
        Assert(projectService.Search(projects, "  ALPHA ").Count == 1, "Case insensitive and trimmed.");
        Assert(projectService.Search(projects, "").Count == 2, "Empty search must return all.");
        Assert(projectService.Search(projects, "Kỹ thuật").Count == 1, "Find department.");
        Assert(projectService.Search(projects, "missing").Count == 0 && projects.Count == 2, "Do not mutate source.");
        Assert(new ProjectDetailService(new FakeProjectDetailDao()).Search(new ArrayList
        {
            new ProjectDetailDto { Id = 9, ProjectName = "Alpha" }
        }, "9").Count == 1, "Find detail ID.");
        Assert(new ProjectTaskService(new FakeProjectTaskDao()).Search(new ArrayList
        {
            new ProjectTaskDto { Priority = "Cao" }
        }, "cao").Count == 1, "Find task priority.");
        Assert(new AssignmentService(new FakeAssignmentDao()).Search(new ArrayList
        {
            new AssignmentDto { EmployeeName = "Nguyễn Văn An" }
        }, "VĂN AN").Count == 1, "Find employee.");
        return Task.CompletedTask;
    }
}

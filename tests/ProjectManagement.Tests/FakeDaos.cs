using DAO.ProjectManagement.Projects;
using DTO.ProjectManagement.Projects;
using DAO.ProjectManagement.ProjectDetails;
using DTO.ProjectManagement.ProjectDetails;
using DAO.ProjectManagement.Tasks;
using DTO.ProjectManagement.Tasks;
using DAO.ProjectManagement.Assignments;
using DTO.ProjectManagement.Assignments;

namespace ProjectManagement.Tests;

// DAO giả chỉ dùng để kiểm tra BUS, không cần kết nối SQL Server.
internal sealed class FakeProjectDao : IProjectDao
{
    public int WriteCount { get; private set; }
    public Task<List<ProjectDto>> GetAllAsync() => Task.FromResult(new List<ProjectDto>());
    public Task<int> InsertAsync(ProjectDto item) { WriteCount++; return Task.FromResult(7); }
    public Task UpdateAsync(ProjectDto item) { WriteCount++; return Task.CompletedTask; }
    public Task DeleteAsync(int id) { WriteCount++; return Task.CompletedTask; }
}

internal sealed class FakeProjectDetailDao : IProjectDetailDao
{
    public int WriteCount { get; private set; }
    public Task<List<ProjectDetailDto>> GetAllAsync() => Task.FromResult(new List<ProjectDetailDto>());
    public Task<int> InsertAsync(ProjectDetailDto item) { WriteCount++; return Task.FromResult(7); }
    public Task UpdateAsync(ProjectDetailDto item) { WriteCount++; return Task.CompletedTask; }
    public Task DeleteAsync(int id) { WriteCount++; return Task.CompletedTask; }
}

internal sealed class FakeProjectTaskDao : IProjectTaskDao
{
    public int WriteCount { get; private set; }
    public Task<List<ProjectTaskDto>> GetAllAsync() => Task.FromResult(new List<ProjectTaskDto>());
    public Task<int> InsertAsync(ProjectTaskDto item) { WriteCount++; return Task.FromResult(7); }
    public Task UpdateAsync(ProjectTaskDto item) { WriteCount++; return Task.CompletedTask; }
    public Task DeleteAsync(int id) { WriteCount++; return Task.CompletedTask; }
}

internal sealed class FakeAssignmentDao : IAssignmentDao
{
    public int WriteCount { get; private set; }
    public Task<List<AssignmentDto>> GetAllAsync() => Task.FromResult(new List<AssignmentDto>());
    public Task<int> InsertAsync(AssignmentDto item) { WriteCount++; return Task.FromResult(7); }
    public Task UpdateAsync(AssignmentDto item) { WriteCount++; return Task.CompletedTask; }
    public Task DeleteAsync(int id) { WriteCount++; return Task.CompletedTask; }
}

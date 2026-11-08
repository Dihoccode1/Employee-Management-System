using DTO.ProjectManagement.Projects;

namespace DAO.ProjectManagement.Projects;

// Cho phép BUS dùng DAO thật hoặc DAO giả khi kiểm thử.
public interface IProjectDao
{
    Task<List<ProjectDto>> GetAllAsync();
    Task<int> InsertAsync(ProjectDto item);
    Task UpdateAsync(ProjectDto item);
    Task DeleteAsync(int id);
}

using DTO.ProjectManagement.Tasks;

namespace DAO.ProjectManagement.Tasks;

// Cho phép BUS dùng DAO thật hoặc DAO giả khi kiểm thử.
public interface IProjectTaskDao
{
    Task<List<ProjectTaskDto>> GetAllAsync();
    Task<int> InsertAsync(ProjectTaskDto item);
    Task UpdateAsync(ProjectTaskDto item);
    Task DeleteAsync(int id);
}

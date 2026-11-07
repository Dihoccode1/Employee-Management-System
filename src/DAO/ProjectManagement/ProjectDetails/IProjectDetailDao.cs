using DTO.ProjectManagement.ProjectDetails;

namespace DAO.ProjectManagement.ProjectDetails;

// Cho phép BUS dùng DAO thật hoặc DAO giả khi kiểm thử.
public interface IProjectDetailDao
{
    Task<List<ProjectDetailDto>> GetAllAsync();
    Task<int> InsertAsync(ProjectDetailDto item);
    Task UpdateAsync(ProjectDetailDto item);
    Task DeleteAsync(int id);
}

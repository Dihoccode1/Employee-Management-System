using DTO.ProjectManagement.Assignments;

namespace DAO.ProjectManagement.Assignments;

// Cho phép BUS dùng DAO thật hoặc DAO giả khi kiểm thử.
public interface IAssignmentDao
{
    Task<List<AssignmentDto>> GetAllAsync();
    Task<int> InsertAsync(AssignmentDto item);
    Task UpdateAsync(AssignmentDto item);
    Task DeleteAsync(int id);
}

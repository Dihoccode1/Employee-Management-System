using System.Collections;
using DAO.ProjectManagement.Tasks;
using DTO.ProjectManagement.Tasks;

namespace BUS.ProjectManagement.Tasks;

public sealed class ProjectTaskService
{
    private readonly IProjectTaskDao _dao;

    public ProjectTaskService(IProjectTaskDao dao)
    {
        _dao = dao;
    }

    public Task<List<ProjectTaskDto>> GetAllAsync()
    {
        return _dao.GetAllAsync();
    }

    public async Task<int> SaveAsync(ProjectTaskDto item)
    {
        ArgumentNullException.ThrowIfNull(item);
        ProjectValidation.RecordId(item.Id);
        item.Status = ProjectValidation.RequiredText(item.Status, "trạng thái", 50);
        ProjectValidation.PositiveId(item.ProjectDetailId, "chi tiết dự án");
        item.Name = ProjectValidation.RequiredText(item.Name, "tên công việc", 200);
        item.Description = ProjectValidation.OptionalText(item.Description, "mô tả", 2000);
        item.Type = ProjectValidation.OptionalText(item.Type, "loại công việc", 100);
        item.Priority = ProjectValidation.OptionalText(item.Priority, "mức độ ưu tiên", 50);
        item.Notes = ProjectValidation.OptionalText(item.Notes, "ghi chú", 2000);
        ProjectValidation.Number(item.EstimatedHours, "Số giờ dự kiến", 99999999.99m);
        ProjectValidation.Number(item.CompletionPercentage, "Tiến độ", 100);
        ProjectValidation.Dates(item.StartDate, item.Deadline, item.CompletedDate);

        // Mã bằng 0 nghĩa là thêm mới; mã lớn hơn 0 nghĩa là cập nhật.
        if (item.Id == 0)
            return await _dao.InsertAsync(item);

        await _dao.UpdateAsync(item);
        return item.Id;
    }

    public Task DeleteAsync(int id)
    {
        ProjectValidation.PositiveId(id, "bản ghi cần xóa");
        return _dao.DeleteAsync(id);
    }

    public List<ProjectTaskDto> Search(ArrayList source, string keyword)
    {
        // Tìm trên ArrayList trong BUS theo yêu cầu môn học.
        keyword = keyword.Trim();
        var result = new List<ProjectTaskDto>();
        foreach (ProjectTaskDto item in source)
        {
            if (item.Id.ToString().Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                item.Status.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                item.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                item.ProjectName.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                item.Type.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                item.Priority.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                result.Add(item);
        }
        return result;
    }
}

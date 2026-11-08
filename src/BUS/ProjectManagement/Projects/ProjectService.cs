using System.Collections;
using DAO.ProjectManagement.Projects;
using DTO.ProjectManagement.Projects;

namespace BUS.ProjectManagement.Projects;

public sealed class ProjectService
{
    private readonly IProjectDao _dao;

    public ProjectService(IProjectDao dao)
    {
        _dao = dao;
    }

    public Task<List<ProjectDto>> GetAllAsync()
    {
        return _dao.GetAllAsync();
    }

    public async Task<int> SaveAsync(ProjectDto item)
    {
        ArgumentNullException.ThrowIfNull(item);
        ProjectValidation.RecordId(item.Id);
        item.Status = ProjectValidation.RequiredText(item.Status, "trạng thái", 50);
        item.Name = ProjectValidation.RequiredText(item.Name, "tên dự án", 200);
        item.Description = ProjectValidation.OptionalText(item.Description, "mô tả", 2000);
        ProjectValidation.PositiveId(item.DepartmentId, "phòng ban");
        ProjectValidation.PositiveId(item.ManagerId, "người quản lý");
        ProjectValidation.Number(item.Budget, "Ngân sách", ProjectValidation.MaximumMoney);
        ProjectValidation.Dates(item.StartDate, item.Deadline, item.EndDate);

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

    public List<ProjectDto> Search(ArrayList source, string keyword)
    {
        // Tìm trên ArrayList trong BUS theo yêu cầu môn học.
        keyword = keyword.Trim();
        var result = new List<ProjectDto>();
        foreach (ProjectDto item in source)
        {
            if (item.Id.ToString().Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                item.Status.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                item.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                item.DepartmentName.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                item.ManagerName.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                result.Add(item);
        }
        return result;
    }
}

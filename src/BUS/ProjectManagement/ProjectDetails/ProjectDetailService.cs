using System.Collections;
using DAO.ProjectManagement.ProjectDetails;
using DTO.ProjectManagement.ProjectDetails;

namespace BUS.ProjectManagement.ProjectDetails;

public sealed class ProjectDetailService
{
    private readonly IProjectDetailDao _dao;

    public ProjectDetailService(IProjectDetailDao dao)
    {
        _dao = dao;
    }

    public Task<List<ProjectDetailDto>> GetAllAsync()
    {
        return _dao.GetAllAsync();
    }

    public async Task<int> SaveAsync(ProjectDetailDto item)
    {
        ArgumentNullException.ThrowIfNull(item);
        ProjectValidation.RecordId(item.Id);
        item.Status = ProjectValidation.RequiredText(item.Status, "trạng thái", 50);
        ProjectValidation.PositiveId(item.ProjectId, "dự án");
        ProjectValidation.Number(item.CompletionPercentage, "Tiến độ", 100);
        ProjectValidation.Number(item.Allowance, "Phụ cấp", ProjectValidation.MaximumMoney);
        ProjectValidation.Number(item.Bonus, "Tiền thưởng", ProjectValidation.MaximumMoney);

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

    public List<ProjectDetailDto> Search(ArrayList source, string keyword)
    {
        // Tìm trên ArrayList trong BUS theo yêu cầu môn học.
        keyword = keyword.Trim();
        var result = new List<ProjectDetailDto>();
        foreach (ProjectDetailDto item in source)
        {
            if (item.Id.ToString().Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                item.Status.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                item.ProjectName.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                result.Add(item);
        }
        return result;
    }
}

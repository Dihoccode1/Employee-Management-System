using System.Collections;
using DAO.ProjectManagement.Assignments;
using DTO.ProjectManagement.Assignments;

namespace BUS.ProjectManagement.Assignments;

public sealed class AssignmentService
{
    private readonly IAssignmentDao _dao;

    public AssignmentService(IAssignmentDao dao)
    {
        _dao = dao;
    }

    public Task<List<AssignmentDto>> GetAllAsync()
    {
        return _dao.GetAllAsync();
    }

    public async Task<int> SaveAsync(AssignmentDto item)
    {
        ArgumentNullException.ThrowIfNull(item);
        ProjectValidation.RecordId(item.Id);
        item.Status = ProjectValidation.RequiredText(item.Status, "trạng thái", 50);
        ProjectValidation.PositiveId(item.TaskId, "công việc");
        ProjectValidation.PositiveId(item.EmployeeId, "nhân viên");
        item.Notes = ProjectValidation.OptionalText(item.Notes, "ghi chú", 2000);
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

    public List<AssignmentDto> Search(ArrayList source, string keyword)
    {
        // Tìm trên ArrayList trong BUS theo yêu cầu môn học.
        keyword = keyword.Trim();
        var result = new List<AssignmentDto>();
        foreach (AssignmentDto item in source)
        {
            if (item.Id.ToString().Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                item.Status.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                item.TaskName.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                item.EmployeeName.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                item.ProjectName.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                result.Add(item);
        }
        return result;
    }
}

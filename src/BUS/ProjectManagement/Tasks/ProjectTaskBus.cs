using System.Collections;
using DAO.ProjectManagement.Tasks;
using DAO.ProjectManagement.ProjectDetails;
using DTO.ProjectManagement.Tasks;

namespace BUS.ProjectManagement.Tasks;

public class ProjectTaskBus
{
    private readonly ProjectTaskDao dataDao;
    private readonly ProjectDetailDao detailDao;
    private ArrayList items = new ArrayList();

    public ProjectTaskBus(ProjectTaskDao dataDao, ProjectDetailDao detailDao)
    {
        this.dataDao = dataDao;
        this.detailDao = detailDao;
    }

    public ArrayList getAll()
    {
        items = new ArrayList(dataDao.getAll());
        return items;
    }

    // Search runs on ArrayList in BUS, not in a SQL WHERE clause.
    public ArrayList search(string keyword)
    {
        getAll();
        string searchText = (keyword ?? "").Trim();
        ArrayList result = new ArrayList();
        foreach (ProjectTaskDto item in items)
        {
            string content = item.id.ToString() + " " + item.projectDetailId + " " + item.name + " " + item.description + " " + item.taskType + " " + item.status + " " + item.priority + " " + item.notes;
            if (content.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                result.Add(item);
        }
        return result;
    }

    public void add(ProjectTaskDto item)
    {
        if (item == null)
            throw new ArgumentException("Dữ liệu không được để trống.");
        item.id = 0;
        validate(item);
        // Change the in-memory list only after SQL Server saves successfully.
        item.id = dataDao.add(item);
        items.Add(item);
    }

    public void update(ProjectTaskDto item)
    {
        if (item == null)
            throw new ArgumentException("Dữ liệu không được để trống.");
        ProjectValidation.validId(item.id);
        validate(item);
        dataDao.update(item);
        for (int index = 0; index < items.Count; index++)
        {
            ProjectTaskDto oldItem = (ProjectTaskDto)items[index]!;
            if (oldItem.id == item.id)
            {
                items[index] = item;
                break;
            }
        }
    }

    public void delete(int id)
    {
        ProjectValidation.validId(id);
        dataDao.delete(id);
        for (int index = 0; index < items.Count; index++)
        {
            if (((ProjectTaskDto)items[index]!).id == id)
            {
                items.RemoveAt(index);
                break;
            }
        }
    }

    public int getProjectId(int taskId)
    {
        return dataDao.getProjectId(taskId);
    }

    public ArrayList getByProjectDetail(int projectDetailId)
    {
        getAll();
        ArrayList result = new ArrayList();
        foreach (ProjectTaskDto item in items)
        {
            if (item.projectDetailId == projectDetailId)
                result.Add(item);
        }
        return result;
    }

    private void validate(ProjectTaskDto item)
    {
        if (!detailDao.exists(item.projectDetailId))
            throw new ArgumentException("Chi tiết dự án không tồn tại.");
        item.name = ProjectValidation.requiredText(item.name, "Tên công việc", 150);
        item.description = ProjectValidation.optionalText(item.description, "Mô tả", 1000);
        item.taskType = ProjectValidation.optionalText(item.taskType, "Loại công việc", 100);
        item.notes = ProjectValidation.optionalText(item.notes, "Ghi chú", 1000);
        if (item.priority != "Low" && item.priority != "Normal" && item.priority != "High")
            throw new ArgumentException("Mức ưu tiên không hợp lệ.");
        ProjectValidation.dateRange(item.startDate, item.deadline, item.completedDate);
        ProjectValidation.amount(item.estimatedHours, 99999999.99m, "Số giờ dự kiến");
        ProjectValidation.amount(item.completionRate, 100m, "Tỷ lệ hoàn thành");
        ProjectValidation.validStatus(item.status);
    }
}

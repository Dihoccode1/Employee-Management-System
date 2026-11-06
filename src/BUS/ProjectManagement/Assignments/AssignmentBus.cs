using System.Collections;
using DAO.ProjectManagement.Assignments;
using DAO.ProjectManagement.Tasks;
using DAO.ProjectManagement.Projects;
using DAO.ProjectManagement;
using DTO.ProjectManagement.Assignments;

namespace BUS.ProjectManagement.Assignments;

public class AssignmentBus
{
    private readonly AssignmentDao dataDao;
    private readonly ProjectTaskDao taskDao;
    private readonly ProjectDao projectDao;
    private readonly ProjectLookupDao lookupDao;
    private ArrayList items = new ArrayList();

    public AssignmentBus(AssignmentDao dataDao, ProjectTaskDao taskDao, ProjectDao projectDao, ProjectLookupDao lookupDao)
    {
        this.dataDao = dataDao;
        this.taskDao = taskDao;
        this.projectDao = projectDao;
        this.lookupDao = lookupDao;
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
        foreach (AssignmentDto item in items)
        {
            string content = item.id.ToString() + " " + item.projectId + " " + item.taskId + " " + item.employeeId + " " + item.status + " " + item.notes;
            if (content.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                result.Add(item);
        }
        return result;
    }

    public void add(AssignmentDto item)
    {
        if (item == null)
            throw new ArgumentException("Dữ liệu không được để trống.");
        item.id = 0;
        validate(item);
        // Change the in-memory list only after SQL Server saves successfully.
        item.id = dataDao.add(item);
        items.Add(item);
    }

    public void update(AssignmentDto item)
    {
        if (item == null)
            throw new ArgumentException("Dữ liệu không được để trống.");
        ProjectValidation.validId(item.id);
        validate(item);
        dataDao.update(item);
        for (int index = 0; index < items.Count; index++)
        {
            AssignmentDto oldItem = (AssignmentDto)items[index]!;
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
            if (((AssignmentDto)items[index]!).id == id)
            {
                items.RemoveAt(index);
                break;
            }
        }
    }

    private void validate(AssignmentDto item)
    {
        if (!projectDao.exists(item.projectId))
            throw new ArgumentException("Dự án không tồn tại.");
        if (taskDao.getProjectId(item.taskId) != item.projectId)
            throw new ArgumentException("Công việc không thuộc dự án đã chọn.");
        if (!lookupDao.employeeExists(item.employeeId))
            throw new ArgumentException("Nhân viên không tồn tại.");
        if (dataDao.assignmentExists(item.taskId, item.employeeId, item.id))
            throw new ArgumentException("Nhân viên này đã được phân công công việc này.");
        if (item.assignedDate.Date > item.startDate.Date)
            throw new ArgumentException("Ngày phân công không được sau ngày bắt đầu.");
        ProjectValidation.dateRange(item.startDate, item.deadline, item.completedDate);
        ProjectValidation.amount(item.completionRate, 100m, "Tỷ lệ hoàn thành");
        ProjectValidation.amount(item.contributionScore, 99999999.99m, "Điểm đóng góp");
        item.notes = ProjectValidation.optionalText(item.notes, "Ghi chú", 1000);
        ProjectValidation.validStatus(item.status);
    }
}

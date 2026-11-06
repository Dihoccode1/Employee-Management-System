using System.Collections;
using DAO.ProjectManagement.Projects;
using DAO.ProjectManagement;
using DTO.ProjectManagement.Projects;

namespace BUS.ProjectManagement.Projects;

public class ProjectBus
{
    private readonly ProjectDao dataDao;
    private readonly ProjectLookupDao lookupDao;
    private ArrayList items = new ArrayList();

    public ProjectBus(ProjectDao dataDao, ProjectLookupDao lookupDao)
    {
        this.dataDao = dataDao;
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
        foreach (ProjectDto item in items)
        {
            string content = item.id.ToString() + " " + item.name + " " + item.description + " " + item.status + " " + item.departmentId + " " + item.managerId;
            if (content.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                result.Add(item);
        }
        return result;
    }

    public void add(ProjectDto item)
    {
        if (item == null)
            throw new ArgumentException("Dữ liệu không được để trống.");
        item.id = 0;
        validate(item);
        // Change the in-memory list only after SQL Server saves successfully.
        item.id = dataDao.add(item);
        items.Add(item);
    }

    public void update(ProjectDto item)
    {
        if (item == null)
            throw new ArgumentException("Dữ liệu không được để trống.");
        ProjectValidation.validId(item.id);
        validate(item);
        dataDao.update(item);
        for (int index = 0; index < items.Count; index++)
        {
            ProjectDto oldItem = (ProjectDto)items[index]!;
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
            if (((ProjectDto)items[index]!).id == id)
            {
                items.RemoveAt(index);
                break;
            }
        }
    }

    private void validate(ProjectDto item)
    {
        item.name = ProjectValidation.requiredText(item.name, "Tên dự án", 150);
        item.description = ProjectValidation.optionalText(item.description, "Mô tả", 1000);
        ProjectValidation.dateRange(item.startDate, item.deadline, item.endDate);
        ProjectValidation.amount(item.budget, 9999999999999999.99m, "Ngân sách");
        if (!lookupDao.departmentExists(item.departmentId))
            throw new ArgumentException("Phòng ban không tồn tại.");
        if (!lookupDao.employeeExists(item.managerId))
            throw new ArgumentException("Người quản lý không tồn tại.");
        ProjectValidation.validStatus(item.status);
    }
}

using System.Collections;
using DAO.ProjectManagement.ProjectDetails;
using DAO.ProjectManagement.Projects;
using DTO.ProjectManagement.ProjectDetails;

namespace BUS.ProjectManagement.ProjectDetails;

public class ProjectDetailBus
{
    private readonly ProjectDetailDao dataDao;
    private readonly ProjectDao projectDao;
    private ArrayList items = new ArrayList();

    public ProjectDetailBus(ProjectDetailDao dataDao, ProjectDao projectDao)
    {
        this.dataDao = dataDao;
        this.projectDao = projectDao;
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
        foreach (ProjectDetailDto item in items)
        {
            string content = item.id.ToString() + " " + item.projectId + " " + item.status;
            if (content.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                result.Add(item);
        }
        return result;
    }

    public void add(ProjectDetailDto item)
    {
        if (item == null)
            throw new ArgumentException("Dữ liệu không được để trống.");
        item.id = 0;
        validate(item);
        // Change the in-memory list only after SQL Server saves successfully.
        item.id = dataDao.add(item);
        items.Add(item);
    }

    public void update(ProjectDetailDto item)
    {
        if (item == null)
            throw new ArgumentException("Dữ liệu không được để trống.");
        ProjectValidation.validId(item.id);
        validate(item);
        dataDao.update(item);
        for (int index = 0; index < items.Count; index++)
        {
            ProjectDetailDto oldItem = (ProjectDetailDto)items[index]!;
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
            if (((ProjectDetailDto)items[index]!).id == id)
            {
                items.RemoveAt(index);
                break;
            }
        }
    }

    private void validate(ProjectDetailDto item)
    {
        if (!projectDao.exists(item.projectId))
            throw new ArgumentException("Dự án không tồn tại.");
        if (dataDao.projectHasDetail(item.projectId, item.id))
            throw new ArgumentException("Dự án này đã có chi tiết. Mỗi dự án chỉ có một chi tiết.");
        ProjectValidation.amount(item.completionRate, 100m, "Tiến độ");
        ProjectValidation.amount(item.allowance, 9999999999999999.99m, "Phụ cấp");
        ProjectValidation.amount(item.reward, 9999999999999999.99m, "Thưởng");
        ProjectValidation.validStatus(item.status);
    }
}

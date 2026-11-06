using DAO.ProjectManagement;
using DTO.ProjectManagement;

namespace BUS.ProjectManagement;

public class ProjectLookupBus
{
    private readonly ProjectLookupDao lookupDao;

    public ProjectLookupBus(ProjectLookupDao lookupDao)
    {
        this.lookupDao = lookupDao;
    }

    public List<LookupItemDto> getDepartments()
    {
        return lookupDao.getDepartments();
    }

    public List<LookupItemDto> getEmployees()
    {
        return lookupDao.getEmployees();
    }
}

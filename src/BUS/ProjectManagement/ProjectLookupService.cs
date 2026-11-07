using DAO.ProjectManagement;
using DTO.ProjectManagement;

namespace BUS.ProjectManagement;

public sealed class ProjectLookupService
{
    private readonly ProjectLookupDao _dao;

    public ProjectLookupService(ProjectLookupDao dao)
    {
        _dao = dao;
    }

    public Task<List<LookupDto>> GetDepartmentsAsync()
    {
        return _dao.GetDepartmentsAsync();
    }

    public Task<List<LookupDto>> GetEmployeesAsync()
    {
        return _dao.GetEmployeesAsync();
    }
}

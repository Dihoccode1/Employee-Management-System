using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DTO.ProjectManagement.Projects;
using DTO.Statistics;

namespace DAO.Statistics;

/// <summary>Hợp đồng truy vấn nguồn dữ liệu phục vụ thống kê ngân sách, phụ cấp và thưởng dự án.</summary>
public interface IProjectStatisticsDao
{
    Task<List<ProjectDto>> GetProjectsAsync(CancellationToken cancellation_token = default);
    Task<List<ProjectDetailStatSourceDto>> GetProjectDetailsForStatsAsync(CancellationToken cancellation_token = default);
}

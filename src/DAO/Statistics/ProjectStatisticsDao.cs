using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DAO.Infrastructure;
using DTO.ProjectManagement.Projects;
using DTO.Statistics;

namespace DAO.Statistics;

/// <summary>DAO thống kê sử dụng MyConnection; tách truy vấn Projects và ProjectDetails để không nhân bản tiền khi nối bảng con.</summary>
public sealed class ProjectStatisticsDao(MyConnection connection_factory) : IProjectStatisticsDao
{
    public async Task<List<ProjectDto>> GetProjectsAsync(CancellationToken cancellation_token = default)
    {
        var items = new List<ProjectDto>();
        using var connection = connection_factory.CreateConnection();
        await connection.OpenAsync(cancellation_token);
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT p.Id,
                   p.Name,
                   p.StartDate,
                   p.EndDate,
                   p.Deadline,
                   p.Description,
                   p.Budget,
                   p.DepartmentId,
                   p.ManagerId,
                   p.Status,
                   d.Code AS DepartmentCode,
                   d.Name AS DepartmentName,
                   e.EmployeeCode AS ManagerCode,
                   e.FullName AS ManagerName
            FROM dbo.Projects p
            JOIN dbo.Departments d ON d.Id = p.DepartmentId
            JOIN dbo.Employees e ON e.Id = p.ManagerId
            ORDER BY p.StartDate ASC, p.Id ASC;
            """;

        using var reader = await command.ExecuteReaderAsync(cancellation_token);
        while (await reader.ReadAsync(cancellation_token))
        {
            items.Add(new ProjectDto
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
                EndDate = reader.IsDBNull(reader.GetOrdinal("EndDate")) ? null : reader.GetDateTime(reader.GetOrdinal("EndDate")),
                Deadline = reader.GetDateTime(reader.GetOrdinal("Deadline")),
                Description = reader.GetString(reader.GetOrdinal("Description")),
                Budget = reader.GetDecimal(reader.GetOrdinal("Budget")),
                DepartmentId = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                ManagerId = reader.GetInt32(reader.GetOrdinal("ManagerId")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                DepartmentCode = reader.GetString(reader.GetOrdinal("DepartmentCode")),
                DepartmentName = reader.GetString(reader.GetOrdinal("DepartmentName")),
                ManagerCode = reader.GetString(reader.GetOrdinal("ManagerCode")),
                ManagerName = reader.GetString(reader.GetOrdinal("ManagerName"))
            });
        }
        return items;
    }

    public async Task<List<ProjectDetailStatSourceDto>> GetProjectDetailsForStatsAsync(CancellationToken cancellation_token = default)
    {
        // Chỉ nối Projects và ProjectDetails để mỗi dòng phụ cấp/thưởng chỉ được tính đúng 1 lần.
        var items = new List<ProjectDetailStatSourceDto>();
        using var connection = connection_factory.CreateConnection();
        await connection.OpenAsync(cancellation_token);
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT pd.Id AS DetailId,
                   p.Id AS ProjectId,
                   p.Name AS ProjectName,
                   p.StartDate AS ProjectStartDate,
                   d.Name AS DepartmentName,
                   e.FullName AS ManagerName,
                   pd.Allowance,
                   pd.Bonus
            FROM dbo.ProjectDetails pd
            JOIN dbo.Projects p ON p.Id = pd.ProjectId
            JOIN dbo.Departments d ON d.Id = p.DepartmentId
            JOIN dbo.Employees e ON e.Id = p.ManagerId
            ORDER BY p.Id ASC, pd.Id ASC;
            """;

        using var reader = await command.ExecuteReaderAsync(cancellation_token);
        while (await reader.ReadAsync(cancellation_token))
        {
            items.Add(new ProjectDetailStatSourceDto
            {
                DetailId = reader.GetInt32(reader.GetOrdinal("DetailId")),
                ProjectId = reader.GetInt32(reader.GetOrdinal("ProjectId")),
                ProjectName = reader.GetString(reader.GetOrdinal("ProjectName")),
                ProjectStartDate = reader.GetDateTime(reader.GetOrdinal("ProjectStartDate")),
                DepartmentName = reader.GetString(reader.GetOrdinal("DepartmentName")),
                ManagerName = reader.GetString(reader.GetOrdinal("ManagerName")),
                Allowance = reader.GetDecimal(reader.GetOrdinal("Allowance")),
                Bonus = reader.GetDecimal(reader.GetOrdinal("Bonus"))
            });
        }
        return items;
    }
}

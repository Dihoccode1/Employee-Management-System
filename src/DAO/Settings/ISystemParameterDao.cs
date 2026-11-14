using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DTO.Settings;

namespace DAO.Settings;

public interface ISystemParameterDao
{
    Task<List<SystemParameterDto>> GetAllAsync(CancellationToken cancellation_token = default);
    Task<SystemParameterDto?> GetByCodeAsync(string code, CancellationToken cancellation_token = default);
    Task<bool> ExistsAsync(string code, CancellationToken cancellation_token = default);
    Task InsertAsync(SystemParameterDto item, CancellationToken cancellation_token = default);
    Task UpdateAsync(SystemParameterDto item, CancellationToken cancellation_token = default);
    Task DeleteAsync(string code, CancellationToken cancellation_token = default);
}

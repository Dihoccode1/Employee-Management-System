using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DTO.HumanResources.Positions;

namespace DAO.HumanResources.Positions;

public interface IPositionDao
{
    Task<List<PositionDto>> GetAllAsync(CancellationToken cancellation_token = default);
    Task<bool> ExistsAsync(string code, CancellationToken cancellation_token = default);
    Task<int> InsertAsync(PositionDto item, CancellationToken cancellation_token = default);
    Task UpdateAsync(PositionDto item, CancellationToken cancellation_token = default);
    Task DeleteAsync(string code, CancellationToken cancellation_token = default);
}

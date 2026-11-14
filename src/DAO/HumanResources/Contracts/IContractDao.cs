using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DTO.HumanResources.Contracts;

namespace DAO.HumanResources.Contracts;

public interface IContractDao
{
    Task<List<ContractDto>> GetAllAsync(CancellationToken cancellation_token = default);
    Task<bool> ExistsAsync(string contract_code, CancellationToken cancellation_token = default);
    Task<int> InsertAsync(ContractDto item, CancellationToken cancellation_token = default);
    Task UpdateAsync(ContractDto item, CancellationToken cancellation_token = default);
    Task DeleteAsync(string contract_code, CancellationToken cancellation_token = default);
}

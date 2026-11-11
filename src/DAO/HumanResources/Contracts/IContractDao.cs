using System.Collections.Generic;
using System.Threading.Tasks;
using DTO.HumanResources.Contracts;
namespace DAO.HumanResources.Contracts;
public interface IContractDao {
    Task<List<ContractDto>> GetAllAsync();
    Task<bool> ExistsAsync(string contractCode);
    Task InsertAsync(ContractDto item);
    Task UpdateAsync(ContractDto item);
    Task DeleteAsync(string contractCode);
}

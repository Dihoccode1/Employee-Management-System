using System.Collections.Generic;
using System.Threading.Tasks;
using DTO.HumanResources.Contracts;
namespace DAO.HumanResources.Contracts;
public interface IContractDao {
    Task<List<ContractDto>> GetAllAsync();
    Task<int> InsertAsync(ContractDto item);
    Task UpdateAsync(ContractDto item);
    Task DeleteAsync(int id);
}

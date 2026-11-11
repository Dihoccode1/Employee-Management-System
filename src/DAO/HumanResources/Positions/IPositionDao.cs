using System.Collections.Generic;
using System.Threading.Tasks;
using DTO.HumanResources.Positions;
namespace DAO.HumanResources.Positions;
public interface IPositionDao {
    Task<List<PositionDto>> GetAllAsync();
    Task<bool> ExistsAsync(string code);
    Task InsertAsync(PositionDto item);
    Task UpdateAsync(PositionDto item);
    Task DeleteAsync(string code);
}

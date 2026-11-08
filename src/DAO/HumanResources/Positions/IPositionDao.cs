using System.Collections.Generic;
using System.Threading.Tasks;
using DTO.HumanResources.Positions;
namespace DAO.HumanResources.Positions;
public interface IPositionDao {
    Task<List<PositionDto>> GetAllAsync();
    Task<int> InsertAsync(PositionDto item);
    Task UpdateAsync(PositionDto item);
    Task DeleteAsync(int id);
}

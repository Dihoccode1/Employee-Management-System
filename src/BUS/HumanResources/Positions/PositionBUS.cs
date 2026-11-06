using System.Collections.Generic;
using HRM.DAO;
using HRM.DTO;

namespace HRM.BUS
{
    public class PositionBUS
    {
        private readonly PositionDAO positionDAO = new PositionDAO();

        public List<PositionDTO> getAllPositions()
        {
            return positionDAO.getAllPositions();
        }

        public bool addPosition(PositionDTO position)
        {
            if (string.IsNullOrWhiteSpace(position.positionId) || string.IsNullOrWhiteSpace(position.positionName) || position.positionAllowance < 0)
                return false;
            return positionDAO.insertPosition(position);
        }

        public bool updatePosition(PositionDTO position)
        {
            if (string.IsNullOrWhiteSpace(position.positionId) || string.IsNullOrWhiteSpace(position.positionName) || position.positionAllowance < 0)
                return false;
            return positionDAO.updatePosition(position);
        }

        public bool deletePosition(string positionId)
        {
            if (string.IsNullOrWhiteSpace(positionId))
                return false;
            return positionDAO.deletePosition(positionId);
        }
    }
}

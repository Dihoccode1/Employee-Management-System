using System.Collections.Generic;
using HRM.DAO.HumanResources;
using HRM.DTO.HumanResources;

namespace HRM.BUS.HumanResources
{
    internal class PositionBUS
    {
        public static List<PositionDTO> positionList;

        public PositionBUS() { }

        public void resetPositionList()
        {
            positionList = new List<PositionDTO>();
        }

        public List<PositionDTO> readPositionList()
        {
            PositionDAO dao = new PositionDAO();
            if (positionList == null || positionList.Count == 0)
            {
                positionList = new List<PositionDTO>();
                positionList = dao.readPositionList();
            }
            return positionList;
        }

        public void add(PositionDTO pos)
        {
            if (string.IsNullOrWhiteSpace(pos.positionId) || string.IsNullOrWhiteSpace(pos.positionName))
                return;
            if (searchById(pos.positionId) != null)
                return;

            PositionDAO dao = new PositionDAO();
            dao.add(pos);
            if (positionList == null) positionList = new List<PositionDTO>();
            positionList.Add(pos);
        }

        public void update(PositionDTO pos)
        {
            PositionDAO dao = new PositionDAO();
            dao.update(pos);
            if (positionList != null)
            {
                for (int i = 0; i < positionList.Count; i++)
                {
                    if (positionList[i].positionId.CompareTo(pos.positionId) == 0)
                    {
                        positionList[i] = pos;
                        break;
                    }
                }
            }
        }

        public void delete(string positionId)
        {
            PositionDAO dao = new PositionDAO();
            dao.delete(positionId);
            PositionDTO existing = searchById(positionId);
            if (existing != null && positionList != null)
            {
                positionList.Remove(existing);
            }
        }

        public PositionDTO searchById(string id)
        {
            if (positionList == null) readPositionList();
            foreach (PositionDTO pos in positionList)
            {
                if (pos.positionId.CompareTo(id) == 0)
                {
                    return pos;
                }
            }
            return null;
        }

        public List<PositionDTO> searchByName(string name)
        {
            if (positionList == null) readPositionList();
            List<PositionDTO> result = new List<PositionDTO>();
            foreach (PositionDTO pos in positionList)
            {
                if (pos.positionName.IndexOf(name) >= 0)
                {
                    result.Add(pos);
                }
            }
            return result;
        }
    }
}

using System.Collections.Generic;
using HRM.DAO.HumanResources;
using HRM.DTO.HumanResources;

namespace HRM.BUS.HumanResources
{
    internal class LaborContractBUS
    {
        public static List<LaborContractDTO> contractList;

        public LaborContractBUS() { }

        public void resetContractList()
        {
            contractList = new List<LaborContractDTO>();
        }

        public List<LaborContractDTO> readContractList()
        {
            LaborContractDAO dao = new LaborContractDAO();
            if (contractList == null || contractList.Count == 0)
            {
                contractList = new List<LaborContractDTO>();
                contractList = dao.readContractList();
            }
            return contractList;
        }

        public void add(LaborContractDTO contract)
        {
            if (string.IsNullOrWhiteSpace(contract.contractId) || string.IsNullOrWhiteSpace(contract.employeeId))
                return;
            if (searchById(contract.contractId) != null)
                return;

            LaborContractDAO dao = new LaborContractDAO();
            dao.add(contract);
            if (contractList == null) contractList = new List<LaborContractDTO>();
            contractList.Add(contract);
        }

        public void update(LaborContractDTO contract)
        {
            LaborContractDAO dao = new LaborContractDAO();
            dao.update(contract);
            if (contractList != null)
            {
                for (int i = 0; i < contractList.Count; i++)
                {
                    if (contractList[i].contractId.CompareTo(contract.contractId) == 0)
                    {
                        contractList[i] = contract;
                        break;
                    }
                }
            }
        }

        public void delete(string contractId)
        {
            LaborContractDAO dao = new LaborContractDAO();
            dao.delete(contractId);
            LaborContractDTO existing = searchById(contractId);
            if (existing != null && contractList != null)
            {
                contractList.Remove(existing);
            }
        }

        public LaborContractDTO searchById(string id)
        {
            if (contractList == null) readContractList();
            foreach (LaborContractDTO contract in contractList)
            {
                if (contract.contractId.CompareTo(id) == 0)
                {
                    return contract;
                }
            }
            return null;
        }

        public List<LaborContractDTO> searchByEmployeeId(string empId)
        {
            if (contractList == null) readContractList();
            List<LaborContractDTO> result = new List<LaborContractDTO>();
            foreach (LaborContractDTO contract in contractList)
            {
                if (contract.employeeId.IndexOf(empId) >= 0)
                {
                    result.Add(contract);
                }
            }
            return result;
        }
    }
}

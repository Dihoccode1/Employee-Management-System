using System.Collections.Generic;
using HRM.DAO;
using HRM.DTO;

namespace HRM.BUS
{
    public class LaborContractBUS
    {
        private readonly LaborContractDAO contractDAO = new LaborContractDAO();

        public List<LaborContractDTO> getAllContracts()
        {
            return contractDAO.getAllContracts();
        }

        public bool addContract(LaborContractDTO contract)
        {
            if (string.IsNullOrWhiteSpace(contract.contractId) || string.IsNullOrWhiteSpace(contract.employeeId) || contract.endDate < contract.startDate)
                return false;
            return contractDAO.insertContract(contract);
        }

        public bool updateContract(LaborContractDTO contract)
        {
            if (string.IsNullOrWhiteSpace(contract.contractId) || string.IsNullOrWhiteSpace(contract.employeeId) || contract.endDate < contract.startDate)
                return false;
            return contractDAO.updateContract(contract);
        }

        public bool deleteContract(string contractId)
        {
            if (string.IsNullOrWhiteSpace(contractId))
                return false;
            return contractDAO.deleteContract(contractId);
        }
    }
}

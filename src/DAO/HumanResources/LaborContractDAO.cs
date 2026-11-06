using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows.Forms;
using HRM.DTO.HumanResources;

namespace HRM.DAO.HumanResources
{
    internal class LaborContractDAO
    {
        private string connString = Environment.GetEnvironmentVariable("HRM_CONNECTION_STRING")
            ?? "Server=localhost;Database=HumanResourceManagement;Integrated Security=True;TrustServerCertificate=True";
        private SqlConnection connection;

        public LaborContractDAO()
        {
            if (connection == null)
            {
                try
                {
                    connection = new SqlConnection(connString);
                    connection.Open();
                    Console.WriteLine("Connection successful!");
                }
                catch
                {
                    MessageBox.Show("Lỗi kết nối CSDL Hợp đồng lao động!");
                }
            }
        }

        public List<LaborContractDTO> readContractList()
        {
            List<LaborContractDTO> contractList = new List<LaborContractDTO>();
            try
            {
                string query = "SELECT ma_hop_dong, ma_nhan_vien, loai_hop_dong, ngay_bat_dau, ngay_ket_thuc, luong_co_ban, trang_thai FROM hop_dong_lao_dong";
                SqlCommand command = new SqlCommand(query, connection);
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    string contractId = reader.GetString(0);
                    string empId = reader["ma_nhan_vien"].ToString();
                    string type = reader["loai_hop_dong"].ToString();
                    DateTime startDate = Convert.ToDateTime(reader["ngay_bat_dau"]);
                    DateTime endDate = Convert.ToDateTime(reader["ngay_ket_thuc"]);
                    decimal salary = Convert.ToDecimal(reader["luong_co_ban"]);
                    string status = reader["trang_thai"].ToString();

                    LaborContractDTO contract = new LaborContractDTO(contractId, empId, type, startDate, endDate, salary, status);
                    contractList.Add(contract);
                }
                reader.Close();
            }
            catch
            {
                MessageBox.Show("Lỗi đọc thông tin Hợp đồng lao động!");
            }
            return contractList;
        }

        public void add(LaborContractDTO contract)
        {
            try
            {
                string query = "INSERT INTO hop_dong_lao_dong VALUES ('" + contract.contractId + "', '" + contract.employeeId + "', N'" + contract.contractType + "', '" + contract.startDate.ToString("yyyy-MM-dd") + "', '" + contract.endDate.ToString("yyyy-MM-dd") + "', " + contract.baseSalary + ", N'" + contract.status + "')";
                SqlCommand command = new SqlCommand(query, connection);
                int r = command.ExecuteNonQuery();
            }
            catch
            {
                MessageBox.Show("Lỗi ghi thông tin Hợp đồng lao động!");
            }
        }

        public void update(LaborContractDTO contract)
        {
            try
            {
                string query = "UPDATE hop_dong_lao_dong SET ma_nhan_vien='" + contract.employeeId + "', loai_hop_dong=N'" + contract.contractType + "', ngay_bat_dau='" + contract.startDate.ToString("yyyy-MM-dd") + "', ngay_ket_thuc='" + contract.endDate.ToString("yyyy-MM-dd") + "', luong_co_ban=" + contract.baseSalary + ", trang_thai=N'" + contract.status + "' WHERE ma_hop_dong='" + contract.contractId + "'";
                SqlCommand command = new SqlCommand(query, connection);
                int r = command.ExecuteNonQuery();
            }
            catch
            {
                MessageBox.Show("Lỗi cập nhật Hợp đồng lao động!");
            }
        }

        public void delete(string contractId)
        {
            try
            {
                string query = "DELETE FROM hop_dong_lao_dong WHERE ma_hop_dong='" + contractId + "'";
                SqlCommand command = new SqlCommand(query, connection);
                int r = command.ExecuteNonQuery();
            }
            catch
            {
                MessageBox.Show("Lỗi xóa Hợp đồng lao động!");
            }
        }

        public void closeConnection()
        {
            if (connection != null) connection.Close();
        }
    }
}

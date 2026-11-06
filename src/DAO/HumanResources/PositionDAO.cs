using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows.Forms;
using HRM.DTO.HumanResources;

namespace HRM.DAO.HumanResources
{
    internal class PositionDAO
    {
        private string connString = Environment.GetEnvironmentVariable("HRM_CONNECTION_STRING")
            ?? "Server=localhost;Database=HumanResourceManagement;Integrated Security=True;TrustServerCertificate=True";
        private SqlConnection connection;

        public PositionDAO()
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
                    MessageBox.Show("Lỗi kết nối CSDL Chức vụ!");
                }
            }
        }

        public List<PositionDTO> readPositionList()
        {
            List<PositionDTO> positionList = new List<PositionDTO>();
            try
            {
                string query = "SELECT ma_chuc_vu, ten_chuc_vu, mo_ta, phu_cap_chuc_vu FROM chuc_vu";
                SqlCommand command = new SqlCommand(query, connection);
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    string id = reader.GetString(0);
                    string name = reader["ten_chuc_vu"].ToString();
                    string desc = reader["mo_ta"] == DBNull.Value ? "" : reader["mo_ta"].ToString();
                    decimal allowance = Convert.ToDecimal(reader["phu_cap_chuc_vu"]);
                    PositionDTO pos = new PositionDTO(id, name, desc, allowance);
                    positionList.Add(pos);
                }
                reader.Close();
            }
            catch
            {
                MessageBox.Show("Lỗi đọc thông tin Chức vụ!");
            }
            return positionList;
        }

        public void add(PositionDTO pos)
        {
            try
            {
                string query = "INSERT INTO chuc_vu VALUES ('" + pos.positionId + "', N'" + pos.positionName + "', N'" + pos.description + "', " + pos.positionAllowance + ")";
                SqlCommand command = new SqlCommand(query, connection);
                int r = command.ExecuteNonQuery();
            }
            catch
            {
                MessageBox.Show("Lỗi ghi thông tin Chức vụ!");
            }
        }

        public void update(PositionDTO pos)
        {
            try
            {
                string query = "UPDATE chuc_vu SET ten_chuc_vu=N'" + pos.positionName + "', mo_ta=N'" + pos.description + "', phu_cap_chuc_vu=" + pos.positionAllowance + " WHERE ma_chuc_vu='" + pos.positionId + "'";
                SqlCommand command = new SqlCommand(query, connection);
                int r = command.ExecuteNonQuery();
            }
            catch
            {
                MessageBox.Show("Lỗi cập nhật Chức vụ!");
            }
        }

        public void delete(string positionId)
        {
            try
            {
                string query = "DELETE FROM chuc_vu WHERE ma_chuc_vu='" + positionId + "'";
                SqlCommand command = new SqlCommand(query, connection);
                int r = command.ExecuteNonQuery();
            }
            catch
            {
                MessageBox.Show("Lỗi xóa Chức vụ!");
            }
        }

        public void closeConnection()
        {
            if (connection != null) connection.Close();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows.Forms;
using HRM.DTO.HumanResources;

namespace HRM.DAO.HumanResources
{
    internal class DepartmentDAO
    {
        private string connString = Environment.GetEnvironmentVariable("HRM_CONNECTION_STRING")
            ?? "Server=localhost;Database=HumanResourceManagement;Integrated Security=True;TrustServerCertificate=True";
        private SqlConnection connection;

        public DepartmentDAO()
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
                    MessageBox.Show("Lỗi kết nối CSDL Phòng ban!");
                }
            }
        }

        public List<DepartmentDTO> readDepartmentList()
        {
            List<DepartmentDTO> departmentList = new List<DepartmentDTO>();
            try
            {
                string query = "SELECT ma_phong_ban, ten_phong_ban, mo_ta FROM phong_ban";
                SqlCommand command = new SqlCommand(query, connection);
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    string id = reader.GetString(0);
                    string name = reader["ten_phong_ban"].ToString();
                    string desc = reader["mo_ta"] == DBNull.Value ? "" : reader["mo_ta"].ToString();
                    DepartmentDTO dept = new DepartmentDTO(id, name, desc);
                    departmentList.Add(dept);
                }
                reader.Close();
            }
            catch
            {
                MessageBox.Show("Lỗi đọc thông tin Phòng ban!");
            }
            return departmentList;
        }

        public void add(DepartmentDTO dept)
        {
            try
            {
                string query = "INSERT INTO phong_ban VALUES ('" + dept.departmentId + "', N'" + dept.departmentName + "', N'" + dept.description + "')";
                SqlCommand command = new SqlCommand(query, connection);
                int r = command.ExecuteNonQuery();
            }
            catch
            {
                MessageBox.Show("Lỗi ghi thông tin Phòng ban!");
            }
        }

        public void update(DepartmentDTO dept)
        {
            try
            {
                string query = "UPDATE phong_ban SET ten_phong_ban=N'" + dept.departmentName + "', mo_ta=N'" + dept.description + "' WHERE ma_phong_ban='" + dept.departmentId + "'";
                SqlCommand command = new SqlCommand(query, connection);
                int r = command.ExecuteNonQuery();
            }
            catch
            {
                MessageBox.Show("Lỗi cập nhật Phòng ban!");
            }
        }

        public void delete(string departmentId)
        {
            try
            {
                string query = "DELETE FROM phong_ban WHERE ma_phong_ban='" + departmentId + "'";
                SqlCommand command = new SqlCommand(query, connection);
                int r = command.ExecuteNonQuery();
            }
            catch
            {
                MessageBox.Show("Lỗi xóa Phòng ban!");
            }
        }

        public void closeConnection()
        {
            if (connection != null) connection.Close();
        }
    }
}

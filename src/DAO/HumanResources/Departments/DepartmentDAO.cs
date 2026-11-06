using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using HRM.DTO;

namespace HRM.DAO
{
    public class DepartmentDAO
    {
        public List<DepartmentDTO> getAllDepartments()
        {
            List<DepartmentDTO> list = new List<DepartmentDTO>();
            string query = "SELECT ma_phong_ban, ten_phong_ban, mo_ta FROM phong_ban";

            using (SqlConnection conn = DatabaseConnection.getConnection())
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new DepartmentDTO(
                            reader["ma_phong_ban"].ToString(),
                            reader["ten_phong_ban"].ToString(),
                            reader["mo_ta"] == DBNull.Value ? "" : reader["mo_ta"].ToString()
                        ));
                    }
                }
            }
            return list;
        }

        public bool insertDepartment(DepartmentDTO department)
        {
            string query = "INSERT INTO phong_ban (ma_phong_ban, ten_phong_ban, mo_ta) VALUES (@id, @name, @desc)";
            using (SqlConnection conn = DatabaseConnection.getConnection())
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@id", department.departmentId);
                cmd.Parameters.AddWithValue("@name", department.departmentName);
                cmd.Parameters.AddWithValue("@desc", string.IsNullOrEmpty(department.description) ? (object)DBNull.Value : department.description);
                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool updateDepartment(DepartmentDTO department)
        {
            string query = "UPDATE phong_ban SET ten_phong_ban = @name, mo_ta = @desc WHERE ma_phong_ban = @id";
            using (SqlConnection conn = DatabaseConnection.getConnection())
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@id", department.departmentId);
                cmd.Parameters.AddWithValue("@name", department.departmentName);
                cmd.Parameters.AddWithValue("@desc", string.IsNullOrEmpty(department.description) ? (object)DBNull.Value : department.description);
                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool deleteDepartment(string departmentId)
        {
            string query = "DELETE FROM phong_ban WHERE ma_phong_ban = @id";
            using (SqlConnection conn = DatabaseConnection.getConnection())
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@id", departmentId);
                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using HRM.DTO;

namespace HRM.DAO
{
    public class EmployeeDAO
    {
        public List<EmployeeDTO> getAllEmployees()
        {
            List<EmployeeDTO> list = new List<EmployeeDTO>();
            string query = @"SELECT ma_nhan_vien, ma_phong_ban, ma_chuc_vu, ho, ten, ngay_sinh, 
                             gioi_tinh, so_dien_thoai, email, ngay_vao_lam, trang_thai, 
                             luong_co_ban_hien_hanh, bang_cap FROM nhan_vien";

            using (SqlConnection conn = DatabaseConnection.getConnection())
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new EmployeeDTO(
                            reader["ma_nhan_vien"].ToString(),
                            reader["ma_phong_ban"].ToString(),
                            reader["ma_chuc_vu"].ToString(),
                            reader["ho"].ToString(),
                            reader["ten"].ToString(),
                            Convert.ToDateTime(reader["ngay_sinh"]),
                            reader["gioi_tinh"].ToString(),
                            reader["so_dien_thoai"].ToString(),
                            reader["email"].ToString(),
                            Convert.ToDateTime(reader["ngay_vao_lam"]),
                            reader["trang_thai"].ToString(),
                            Convert.ToDecimal(reader["luong_co_ban_hien_hanh"]),
                            reader["bang_cap"] == DBNull.Value ? null : reader["bang_cap"].ToString()
                        ));
                    }
                }
            }
            return list;
        }

        public bool insertEmployee(EmployeeDTO emp)
        {
            string query = @"INSERT INTO nhan_vien (ma_nhan_vien, ma_phong_ban, ma_chuc_vu, ho, ten, 
                             ngay_sinh, gioi_tinh, so_dien_thoai, email, ngay_vao_lam, trang_thai, 
                             luong_co_ban_hien_hanh, bang_cap) 
                             VALUES (@id, @deptId, @posId, @lastName, @firstName, @dob, @gender, 
                             @phone, @email, @hireDate, @status, @salary, @degree)";

            using (SqlConnection conn = DatabaseConnection.getConnection())
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@id", emp.employeeId);
                cmd.Parameters.AddWithValue("@deptId", emp.departmentId);
                cmd.Parameters.AddWithValue("@posId", emp.positionId);
                cmd.Parameters.AddWithValue("@lastName", emp.lastName);
                cmd.Parameters.AddWithValue("@firstName", emp.firstName);
                cmd.Parameters.AddWithValue("@dob", emp.dateOfBirth);
                cmd.Parameters.AddWithValue("@gender", emp.gender);
                cmd.Parameters.AddWithValue("@phone", emp.phoneNumber);
                cmd.Parameters.AddWithValue("@email", emp.email);
                cmd.Parameters.AddWithValue("@hireDate", emp.hireDate);
                cmd.Parameters.AddWithValue("@status", emp.status);
                cmd.Parameters.AddWithValue("@salary", emp.currentBaseSalary);
                cmd.Parameters.AddWithValue("@degree", string.IsNullOrEmpty(emp.degree) ? (object)DBNull.Value : emp.degree);

                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool updateEmployee(EmployeeDTO emp)
        {
            string query = @"UPDATE nhan_vien SET ma_phong_ban = @deptId, ma_chuc_vu = @posId, 
                             ho = @lastName, ten = @firstName, ngay_sinh = @dob, gioi_tinh = @gender, 
                             so_dien_thoai = @phone, email = @email, ngay_vao_lam = @hireDate, 
                             trang_thai = @status, luong_co_ban_hien_hanh = @salary, bang_cap = @degree 
                             WHERE ma_nhan_vien = @id";

            using (SqlConnection conn = DatabaseConnection.getConnection())
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@id", emp.employeeId);
                cmd.Parameters.AddWithValue("@deptId", emp.departmentId);
                cmd.Parameters.AddWithValue("@posId", emp.positionId);
                cmd.Parameters.AddWithValue("@lastName", emp.lastName);
                cmd.Parameters.AddWithValue("@firstName", emp.firstName);
                cmd.Parameters.AddWithValue("@dob", emp.dateOfBirth);
                cmd.Parameters.AddWithValue("@gender", emp.gender);
                cmd.Parameters.AddWithValue("@phone", emp.phoneNumber);
                cmd.Parameters.AddWithValue("@email", emp.email);
                cmd.Parameters.AddWithValue("@hireDate", emp.hireDate);
                cmd.Parameters.AddWithValue("@status", emp.status);
                cmd.Parameters.AddWithValue("@salary", emp.currentBaseSalary);
                cmd.Parameters.AddWithValue("@degree", string.IsNullOrEmpty(emp.degree) ? (object)DBNull.Value : emp.degree);

                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool deleteEmployee(string employeeId)
        {
            string query = "DELETE FROM nhan_vien WHERE ma_nhan_vien = @id";
            using (SqlConnection conn = DatabaseConnection.getConnection())
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@id", employeeId);
                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }
    }
}

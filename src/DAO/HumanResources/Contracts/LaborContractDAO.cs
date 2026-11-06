using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using HRM.DTO;

namespace HRM.DAO
{
    public class LaborContractDAO
    {
        public List<LaborContractDTO> getAllContracts()
        {
            List<LaborContractDTO> list = new List<LaborContractDTO>();
            string query = @"SELECT ma_hop_dong, ma_nhan_vien, loai_hop_dong, 
                             ngay_bat_dau, ngay_ket_thuc, luong_co_ban, trang_thai 
                             FROM hop_dong_lao_dong";

            using (SqlConnection conn = DatabaseConnection.getConnection())
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new LaborContractDTO(
                            reader["ma_hop_dong"].ToString(),
                            reader["ma_nhan_vien"].ToString(),
                            reader["loai_hop_dong"].ToString(),
                            Convert.ToDateTime(reader["ngay_bat_dau"]),
                            Convert.ToDateTime(reader["ngay_ket_thuc"]),
                            Convert.ToDecimal(reader["luong_co_ban"]),
                            reader["trang_thai"].ToString()
                        ));
                    }
                }
            }
            return list;
        }

        public bool insertContract(LaborContractDTO contract)
        {
            string query = @"INSERT INTO hop_dong_lao_dong (ma_hop_dong, ma_nhan_vien, loai_hop_dong, 
                             ngay_bat_dau, ngay_ket_thuc, luong_co_ban, trang_thai) 
                             VALUES (@id, @empId, @type, @startDate, @endDate, @salary, @status)";

            using (SqlConnection conn = DatabaseConnection.getConnection())
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@id", contract.contractId);
                cmd.Parameters.AddWithValue("@empId", contract.employeeId);
                cmd.Parameters.AddWithValue("@type", contract.contractType);
                cmd.Parameters.AddWithValue("@startDate", contract.startDate);
                cmd.Parameters.AddWithValue("@endDate", contract.endDate);
                cmd.Parameters.AddWithValue("@salary", contract.baseSalary);
                cmd.Parameters.AddWithValue("@status", contract.status);

                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool updateContract(LaborContractDTO contract)
        {
            string query = @"UPDATE hop_dong_lao_dong SET ma_nhan_vien = @empId, loai_hop_dong = @type, 
                             ngay_bat_dau = @startDate, ngay_ket_thuc = @endDate, 
                             luong_co_ban = @salary, trang_thai = @status 
                             WHERE ma_hop_dong = @id";

            using (SqlConnection conn = DatabaseConnection.getConnection())
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@id", contract.contractId);
                cmd.Parameters.AddWithValue("@empId", contract.employeeId);
                cmd.Parameters.AddWithValue("@type", contract.contractType);
                cmd.Parameters.AddWithValue("@startDate", contract.startDate);
                cmd.Parameters.AddWithValue("@endDate", contract.endDate);
                cmd.Parameters.AddWithValue("@salary", contract.baseSalary);
                cmd.Parameters.AddWithValue("@status", contract.status);

                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool deleteContract(string contractId)
        {
            string query = "DELETE FROM hop_dong_lao_dong WHERE ma_hop_dong = @id";
            using (SqlConnection conn = DatabaseConnection.getConnection())
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@id", contractId);
                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }
    }
}

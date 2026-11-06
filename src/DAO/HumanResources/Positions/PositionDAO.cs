using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using HRM.DTO;

namespace HRM.DAO
{
    public class PositionDAO
    {
        public List<PositionDTO> getAllPositions()
        {
            List<PositionDTO> list = new List<PositionDTO>();
            string query = "SELECT ma_chuc_vu, ten_chuc_vu, mo_ta, phu_cap_chuc_vu FROM chuc_vu";

            using (SqlConnection conn = DatabaseConnection.getConnection())
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new PositionDTO(
                            reader["ma_chuc_vu"].ToString(),
                            reader["ten_chuc_vu"].ToString(),
                            reader["mo_ta"] == DBNull.Value ? "" : reader["mo_ta"].ToString(),
                            Convert.ToDecimal(reader["phu_cap_chuc_vu"])
                        ));
                    }
                }
            }
            return list;
        }

        public bool insertPosition(PositionDTO position)
        {
            string query = "INSERT INTO chuc_vu (ma_chuc_vu, ten_chuc_vu, mo_ta, phu_cap_chuc_vu) VALUES (@id, @name, @desc, @allowance)";
            using (SqlConnection conn = DatabaseConnection.getConnection())
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@id", position.positionId);
                cmd.Parameters.AddWithValue("@name", position.positionName);
                cmd.Parameters.AddWithValue("@desc", string.IsNullOrEmpty(position.description) ? (object)DBNull.Value : position.description);
                cmd.Parameters.AddWithValue("@allowance", position.positionAllowance);
                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool updatePosition(PositionDTO position)
        {
            string query = "UPDATE chuc_vu SET ten_chuc_vu = @name, mo_ta = @desc, phu_cap_chuc_vu = @allowance WHERE ma_chuc_vu = @id";
            using (SqlConnection conn = DatabaseConnection.getConnection())
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@id", position.positionId);
                cmd.Parameters.AddWithValue("@name", position.positionName);
                cmd.Parameters.AddWithValue("@desc", string.IsNullOrEmpty(position.description) ? (object)DBNull.Value : position.description);
                cmd.Parameters.AddWithValue("@allowance", position.positionAllowance);
                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool deletePosition(string positionId)
        {
            string query = "DELETE FROM chuc_vu WHERE ma_chuc_vu = @id";
            using (SqlConnection conn = DatabaseConnection.getConnection())
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@id", positionId);
                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }
    }
}

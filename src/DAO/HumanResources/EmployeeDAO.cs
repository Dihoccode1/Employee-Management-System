using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows.Forms;
using HRM.DTO.HumanResources;

namespace HRM.DAO.HumanResources
{
    internal class EmployeeDAO
    {
        private string connString = Environment.GetEnvironmentVariable("HRM_CONNECTION_STRING")
            ?? "Server=localhost;Database=HumanResourceManagement;Integrated Security=True;TrustServerCertificate=True";
        private SqlConnection connection;

        public EmployeeDAO()
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
                    MessageBox.Show("Lỗi kết nối CSDL Nhân viên!");
                }
            }
        }

        public List<EmployeeDTO> readEmployeeList()
        {
            List<EmployeeDTO> employeeList = new List<EmployeeDTO>();
            try
            {
                string query = "SELECT ma_nhan_vien, ma_phong_ban, ma_chuc_vu, ho, ten, ngay_sinh, gioi_tinh, so_dien_thoai, email, ngay_vao_lam, trang_thai, luong_co_ban_hien_hanh, bang_cap FROM nhan_vien";
                SqlCommand command = new SqlCommand(query, connection);
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    string empId = reader.GetString(0);
                    string deptId = reader["ma_phong_ban"].ToString();
                    string posId = reader["ma_chuc_vu"].ToString();
                    string lastName = reader["ho"].ToString();
                    string firstName = reader["ten"].ToString();
                    DateTime dob = Convert.ToDateTime(reader["ngay_sinh"]);
                    string gender = reader["gioi_tinh"].ToString();
                    string phone = reader["so_dien_thoai"].ToString();
                    string email = reader["email"].ToString();
                    DateTime hireDate = Convert.ToDateTime(reader["ngay_vao_lam"]);
                    string status = reader["trang_thai"].ToString();
                    decimal salary = Convert.ToDecimal(reader["luong_co_ban_hien_hanh"]);
                    string degree = reader["bang_cap"] == DBNull.Value ? "" : reader["bang_cap"].ToString();

                    EmployeeDTO emp = new EmployeeDTO(empId, deptId, posId, lastName, firstName, dob, gender, phone, email, hireDate, status, salary, degree);
                    employeeList.Add(emp);
                }
                reader.Close();
            }
            catch
            {
                MessageBox.Show("Lỗi đọc thông tin Nhân viên!");
            }
            return employeeList;
        }

        public void add(EmployeeDTO emp)
        {
            try
            {
                string query = "INSERT INTO nhan_vien VALUES ('" + emp.employeeId + "', '" + emp.departmentId + "', '" + emp.positionId + "', N'" + emp.lastName + "', N'" + emp.firstName + "', '" + emp.dateOfBirth.ToString("yyyy-MM-dd") + "', N'" + emp.gender + "', '" + emp.phoneNumber + "', '" + emp.email + "', '" + emp.hireDate.ToString("yyyy-MM-dd") + "', N'" + emp.status + "', " + emp.currentBaseSalary + ", N'" + emp.degree + "')";
                SqlCommand command = new SqlCommand(query, connection);
                int r = command.ExecuteNonQuery();
            }
            catch
            {
                MessageBox.Show("Lỗi ghi thông tin Nhân viên!");
            }
        }

        public void update(EmployeeDTO emp)
        {
            try
            {
                string query = "UPDATE nhan_vien SET ma_phong_ban='" + emp.departmentId + "', ma_chuc_vu='" + emp.positionId + "', ho=N'" + emp.lastName + "', ten=N'" + emp.firstName + "', ngay_sinh='" + emp.dateOfBirth.ToString("yyyy-MM-dd") + "', gioi_tinh=N'" + emp.gender + "', so_dien_thoai='" + emp.phoneNumber + "', email='" + emp.email + "', ngay_vao_lam='" + emp.hireDate.ToString("yyyy-MM-dd") + "', trang_thai=N'" + emp.status + "', luong_co_ban_hien_hanh=" + emp.currentBaseSalary + ", bang_cap=N'" + emp.degree + "' WHERE ma_nhan_vien='" + emp.employeeId + "'";
                SqlCommand command = new SqlCommand(query, connection);
                int r = command.ExecuteNonQuery();
            }
            catch
            {
                MessageBox.Show("Lỗi cập nhật Nhân viên!");
            }
        }

        public void delete(string employeeId)
        {
            try
            {
                string query = "DELETE FROM nhan_vien WHERE ma_nhan_vien='" + employeeId + "'";
                SqlCommand command = new SqlCommand(query, connection);
                int r = command.ExecuteNonQuery();
            }
            catch
            {
                MessageBox.Show("Lỗi xóa Nhân viên!");
            }
        }

        public void closeConnection()
        {
            if (connection != null) connection.Close();
        }
    }
}

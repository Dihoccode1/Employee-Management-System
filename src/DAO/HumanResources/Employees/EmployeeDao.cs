using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using DAO.Infrastructure;
using DTO.HumanResources.Employees;
namespace DAO.HumanResources.Employees;
public sealed class EmployeeDao(MyConnection connectionFactory) : IEmployeeDao {
    public async Task<List<EmployeeDto>> GetAllAsync() {
        var items = new List<EmployeeDto>();
        using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandText = @"SELECT e.Id, e.EmployeeCode, e.FullName, e.Email, e.DepartmentId, e.PositionId, e.HireDate, e.IsActive,
                                       ISNULL(d.Name, N'') AS DepartmentName, ISNULL(p.Name, N'') AS PositionName
                                FROM dbo.Employees e
                                LEFT JOIN dbo.Departments d ON d.Id = e.DepartmentId
                                LEFT JOIN dbo.Positions p ON p.Id = e.PositionId
                                ORDER BY e.Id DESC;";
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync()) {
            items.Add(new EmployeeDto {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                EmployeeCode = reader.GetString(reader.GetOrdinal("EmployeeCode")),
                FullName = reader.GetString(reader.GetOrdinal("FullName")),
                Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? null : reader.GetString(reader.GetOrdinal("Email")),
                DepartmentId = reader.IsDBNull(reader.GetOrdinal("DepartmentId")) ? null : reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                PositionId = reader.IsDBNull(reader.GetOrdinal("PositionId")) ? null : reader.GetInt32(reader.GetOrdinal("PositionId")),
                HireDate = reader.GetDateTime(reader.GetOrdinal("HireDate")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                DepartmentName = reader.GetString(reader.GetOrdinal("DepartmentName")),
                PositionName = reader.GetString(reader.GetOrdinal("PositionName"))
            });
        }
        return items;
    }
    public async Task<int> InsertAsync(EmployeeDto item) {
        using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandText = @"INSERT INTO dbo.Employees (EmployeeCode, FullName, Email, DepartmentId, PositionId, HireDate, IsActive)
                                OUTPUT INSERTED.Id
                                VALUES (@EmployeeCode, @FullName, @Email, @DepartmentId, @PositionId, @HireDate, @IsActive);";
        AddParameters(command, item);
        try { return Convert.ToInt32(await command.ExecuteScalarAsync()); }
        catch (SqlException ex) { throw HrSqlParameterHelper.CreateWriteException(ex); }
    }
    public async Task UpdateAsync(EmployeeDto item) {
        using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandText = @"UPDATE dbo.Employees SET EmployeeCode = @EmployeeCode, FullName = @FullName, Email = @Email,
                                DepartmentId = @DepartmentId, PositionId = @PositionId, HireDate = @HireDate, IsActive = @IsActive WHERE Id = @Id;";
        command.Parameters.Add("@Id", SqlDbType.Int).Value = item.Id;
        AddParameters(command, item);
        try { if (await command.ExecuteNonQueryAsync() == 0) throw new InvalidOperationException("Bản ghi không tồn tại."); }
        catch (SqlException ex) { throw HrSqlParameterHelper.CreateWriteException(ex); }
    }
    public async Task DeleteAsync(int id) {
        using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM dbo.Employees WHERE Id = @Id;";
        command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
        try { if (await command.ExecuteNonQueryAsync() == 0) throw new InvalidOperationException("Bản ghi không tồn tại."); }
        catch (SqlException ex) when (ex.Number == 547) { throw new InvalidOperationException("Không thể xóa vì còn dữ liệu liên quan.", ex); }
    }
    private static void AddParameters(SqlCommand command, EmployeeDto item) {
        command.Parameters.Add("@EmployeeCode", SqlDbType.VarChar, 20).Value = item.EmployeeCode;
        command.Parameters.Add("@FullName", SqlDbType.NVarChar, 150).Value = item.FullName;
        command.Parameters.Add("@Email", SqlDbType.NVarChar, 254).Value = (object?)item.Email ?? DBNull.Value;
        command.Parameters.Add("@DepartmentId", SqlDbType.Int).Value = (object?)item.DepartmentId ?? DBNull.Value;
        command.Parameters.Add("@PositionId", SqlDbType.Int).Value = (object?)item.PositionId ?? DBNull.Value;
        command.Parameters.Add("@HireDate", SqlDbType.Date).Value = item.HireDate.Date;
        command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = item.IsActive;
    }
}

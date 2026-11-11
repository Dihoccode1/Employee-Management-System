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
        command.CommandText = @"SELECT e.EmployeeCode, e.FullName, e.Email,
                                       ISNULL(d.Code, '') AS DepartmentCode, ISNULL(d.Name, N'') AS DepartmentName,
                                       ISNULL(p.Code, '') AS PositionCode, ISNULL(p.Name, N'') AS PositionName,
                                       e.HireDate, e.IsActive
                                FROM dbo.Employees e
                                LEFT JOIN dbo.Departments d ON d.Id = e.DepartmentId
                                LEFT JOIN dbo.Positions p ON p.Id = e.PositionId
                                ORDER BY e.EmployeeCode ASC;";
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync()) {
            items.Add(new EmployeeDto {
                EmployeeCode = reader.GetString(reader.GetOrdinal("EmployeeCode")),
                FullName = reader.GetString(reader.GetOrdinal("FullName")),
                Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? null : reader.GetString(reader.GetOrdinal("Email")),
                DepartmentCode = reader.GetString(reader.GetOrdinal("DepartmentCode")),
                DepartmentName = reader.GetString(reader.GetOrdinal("DepartmentName")),
                PositionCode = reader.GetString(reader.GetOrdinal("PositionCode")),
                PositionName = reader.GetString(reader.GetOrdinal("PositionName")),
                HireDate = reader.GetDateTime(reader.GetOrdinal("HireDate")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
            });
        }
        return items;
    }
    public async Task<bool> ExistsAsync(string employeeCode) {
        using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(1) FROM dbo.Employees WHERE EmployeeCode = @EmployeeCode;";
        command.Parameters.Add("@EmployeeCode", SqlDbType.VarChar, 20).Value = employeeCode;
        return Convert.ToInt32(await command.ExecuteScalarAsync()) > 0;
    }
    public async Task InsertAsync(EmployeeDto item) {
        using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandText = @"
            DECLARE @DeptId INT = (SELECT TOP 1 Id FROM dbo.Departments WHERE Code = @DepartmentCode OR TRY_CAST(@DepartmentCode AS INT) = Id);
            DECLARE @PosId INT = (SELECT TOP 1 Id FROM dbo.Positions WHERE Code = @PositionCode OR TRY_CAST(@PositionCode AS INT) = Id);
            INSERT INTO dbo.Employees (EmployeeCode, FullName, Email, DepartmentId, PositionId, HireDate, IsActive)
            VALUES (@EmployeeCode, @FullName, @Email, @DeptId, @PosId, @HireDate, @IsActive);";
        AddParameters(command, item);
        try { await command.ExecuteNonQueryAsync(); }
        catch (SqlException ex) { throw HrSqlParameterHelper.CreateWriteException(ex); }
    }
    public async Task UpdateAsync(EmployeeDto item) {
        using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandText = @"
            DECLARE @DeptId INT = (SELECT TOP 1 Id FROM dbo.Departments WHERE Code = @DepartmentCode OR TRY_CAST(@DepartmentCode AS INT) = Id);
            DECLARE @PosId INT = (SELECT TOP 1 Id FROM dbo.Positions WHERE Code = @PositionCode OR TRY_CAST(@PositionCode AS INT) = Id);
            UPDATE dbo.Employees
            SET FullName = @FullName, Email = @Email, DepartmentId = @DeptId, PositionId = @PosId, HireDate = @HireDate, IsActive = @IsActive
            WHERE EmployeeCode = @EmployeeCode;";
        AddParameters(command, item);
        try { if (await command.ExecuteNonQueryAsync() == 0) throw new InvalidOperationException("Mã nhân viên không tồn tại."); }
        catch (SqlException ex) { throw HrSqlParameterHelper.CreateWriteException(ex); }
    }
    public async Task DeleteAsync(string employeeCode) {
        using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM dbo.Employees WHERE EmployeeCode = @EmployeeCode;";
        command.Parameters.Add("@EmployeeCode", SqlDbType.VarChar, 20).Value = employeeCode;
        try { if (await command.ExecuteNonQueryAsync() == 0) throw new InvalidOperationException("Mã nhân viên không tồn tại."); }
        catch (SqlException ex) when (ex.Number == 547) { throw new InvalidOperationException("Không thể xóa vì còn dữ liệu liên quan.", ex); }
    }
    private static void AddParameters(SqlCommand command, EmployeeDto item) {
        command.Parameters.Add("@EmployeeCode", SqlDbType.VarChar, 20).Value = item.EmployeeCode;
        command.Parameters.Add("@FullName", SqlDbType.NVarChar, 150).Value = item.FullName;
        command.Parameters.Add("@Email", SqlDbType.NVarChar, 254).Value = (object?)item.Email ?? DBNull.Value;
        command.Parameters.Add("@DepartmentCode", SqlDbType.VarChar, 20).Value = item.DepartmentCode ?? string.Empty;
        command.Parameters.Add("@PositionCode", SqlDbType.VarChar, 20).Value = item.PositionCode ?? string.Empty;
        command.Parameters.Add("@HireDate", SqlDbType.Date).Value = item.HireDate.Date;
        command.Parameters.Add("@IsActive", SqlDbType.Bit).Value = item.IsActive;
    }
}

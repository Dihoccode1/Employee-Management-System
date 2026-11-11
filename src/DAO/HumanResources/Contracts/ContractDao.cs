using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using DAO.Infrastructure;
using DTO.HumanResources.Contracts;
namespace DAO.HumanResources.Contracts;
public sealed class ContractDao(MyConnection connectionFactory) : IContractDao {
    public async Task<List<ContractDto>> GetAllAsync() {
        var items = new List<ContractDto>();
        using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandText = @"SELECT c.ContractCode, e.EmployeeCode, e.FullName AS EmployeeName, c.StartDate, c.EndDate, c.BaseSalary
                                FROM dbo.Contracts c JOIN dbo.Employees e ON e.Id = c.EmployeeId ORDER BY c.ContractCode ASC;";
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync()) {
            items.Add(new ContractDto {
                ContractCode = reader.GetString(reader.GetOrdinal("ContractCode")),
                EmployeeCode = reader.GetString(reader.GetOrdinal("EmployeeCode")),
                EmployeeName = reader.GetString(reader.GetOrdinal("EmployeeName")),
                StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
                EndDate = reader.IsDBNull(reader.GetOrdinal("EndDate")) ? null : reader.GetDateTime(reader.GetOrdinal("EndDate")),
                BaseSalary = reader.GetDecimal(reader.GetOrdinal("BaseSalary"))
            });
        }
        return items;
    }
    public async Task<bool> ExistsAsync(string contractCode) {
        using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(1) FROM dbo.Contracts WHERE ContractCode = @ContractCode;";
        command.Parameters.Add("@ContractCode", SqlDbType.VarChar, 30).Value = contractCode;
        return Convert.ToInt32(await command.ExecuteScalarAsync()) > 0;
    }
    public async Task InsertAsync(ContractDto item) {
        using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandText = @"
            DECLARE @EmpId INT = (SELECT TOP 1 Id FROM dbo.Employees WHERE EmployeeCode = @EmployeeCode OR TRY_CAST(@EmployeeCode AS INT) = Id);
            IF @EmpId IS NULL THROW 50001, N'Mã nhân viên không tồn tại.', 1;
            INSERT INTO dbo.Contracts (EmployeeId, ContractCode, StartDate, EndDate, BaseSalary)
            VALUES (@EmpId, @ContractCode, @StartDate, @EndDate, @BaseSalary);";
        AddParameters(command, item);
        try { await command.ExecuteNonQueryAsync(); }
        catch (SqlException ex) { throw HrSqlParameterHelper.CreateWriteException(ex); }
    }
    public async Task UpdateAsync(ContractDto item) {
        using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandText = @"
            DECLARE @EmpId INT = (SELECT TOP 1 Id FROM dbo.Employees WHERE EmployeeCode = @EmployeeCode OR TRY_CAST(@EmployeeCode AS INT) = Id);
            IF @EmpId IS NULL THROW 50001, N'Mã nhân viên không tồn tại.', 1;
            UPDATE dbo.Contracts SET EmployeeId = @EmpId, StartDate = @StartDate, EndDate = @EndDate, BaseSalary = @BaseSalary
            WHERE ContractCode = @ContractCode;";
        AddParameters(command, item);
        try { if (await command.ExecuteNonQueryAsync() == 0) throw new InvalidOperationException("Mã hợp đồng không tồn tại."); }
        catch (SqlException ex) { throw HrSqlParameterHelper.CreateWriteException(ex); }
    }
    public async Task DeleteAsync(string contractCode) {
        using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM dbo.Contracts WHERE ContractCode = @ContractCode;";
        command.Parameters.Add("@ContractCode", SqlDbType.VarChar, 30).Value = contractCode;
        try { if (await command.ExecuteNonQueryAsync() == 0) throw new InvalidOperationException("Mã hợp đồng không tồn tại."); }
        catch (SqlException ex) when (ex.Number == 547) { throw new InvalidOperationException("Không thể xóa vì còn dữ liệu liên quan.", ex); }
    }
    private static void AddParameters(SqlCommand command, ContractDto item) {
        command.Parameters.Add("@ContractCode", SqlDbType.VarChar, 30).Value = item.ContractCode;
        command.Parameters.Add("@EmployeeCode", SqlDbType.VarChar, 20).Value = item.EmployeeCode;
        command.Parameters.Add("@StartDate", SqlDbType.Date).Value = item.StartDate.Date;
        command.Parameters.Add("@EndDate", SqlDbType.Date).Value = (object?)item.EndDate?.Date ?? DBNull.Value;
        HrSqlParameterHelper.AddDecimal(command, "@BaseSalary", item.BaseSalary, 18, 2);
    }
}

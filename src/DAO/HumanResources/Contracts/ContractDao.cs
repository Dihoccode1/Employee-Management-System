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
        command.CommandText = @"SELECT c.Id, c.EmployeeId, c.ContractCode, c.StartDate, c.EndDate, c.BaseSalary, e.FullName AS EmployeeName
                                FROM dbo.Contracts c JOIN dbo.Employees e ON e.Id = c.EmployeeId ORDER BY c.Id DESC;";
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync()) {
            items.Add(new ContractDto {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                EmployeeId = reader.GetInt32(reader.GetOrdinal("EmployeeId")),
                ContractCode = reader.GetString(reader.GetOrdinal("ContractCode")),
                StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
                EndDate = reader.IsDBNull(reader.GetOrdinal("EndDate")) ? null : reader.GetDateTime(reader.GetOrdinal("EndDate")),
                BaseSalary = reader.GetDecimal(reader.GetOrdinal("BaseSalary")),
                EmployeeName = reader.GetString(reader.GetOrdinal("EmployeeName"))
            });
        }
        return items;
    }
    public async Task<int> InsertAsync(ContractDto item) {
        using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandText = @"INSERT INTO dbo.Contracts (EmployeeId, ContractCode, StartDate, EndDate, BaseSalary)
                                OUTPUT INSERTED.Id VALUES (@EmployeeId, @ContractCode, @StartDate, @EndDate, @BaseSalary);";
        AddParameters(command, item);
        try { return Convert.ToInt32(await command.ExecuteScalarAsync()); }
        catch (SqlException ex) { throw HrSqlParameterHelper.CreateWriteException(ex); }
    }
    public async Task UpdateAsync(ContractDto item) {
        using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandText = @"UPDATE dbo.Contracts SET EmployeeId = @EmployeeId, ContractCode = @ContractCode,
                                StartDate = @StartDate, EndDate = @EndDate, BaseSalary = @BaseSalary WHERE Id = @Id;";
        command.Parameters.Add("@Id", SqlDbType.Int).Value = item.Id;
        AddParameters(command, item);
        try { if (await command.ExecuteNonQueryAsync() == 0) throw new InvalidOperationException("Bản ghi không tồn tại."); }
        catch (SqlException ex) { throw HrSqlParameterHelper.CreateWriteException(ex); }
    }
    public async Task DeleteAsync(int id) {
        using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM dbo.Contracts WHERE Id = @Id;";
        command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
        try { if (await command.ExecuteNonQueryAsync() == 0) throw new InvalidOperationException("Bản ghi không tồn tại."); }
        catch (SqlException ex) when (ex.Number == 547) { throw new InvalidOperationException("Không thể xóa vì còn dữ liệu liên quan.", ex); }
    }
    private static void AddParameters(SqlCommand command, ContractDto item) {
        command.Parameters.Add("@EmployeeId", SqlDbType.Int).Value = item.EmployeeId;
        command.Parameters.Add("@ContractCode", SqlDbType.VarChar, 30).Value = item.ContractCode;
        command.Parameters.Add("@StartDate", SqlDbType.Date).Value = item.StartDate.Date;
        command.Parameters.Add("@EndDate", SqlDbType.Date).Value = (object?)item.EndDate?.Date ?? DBNull.Value;
        HrSqlParameterHelper.AddDecimal(command, "@BaseSalary", item.BaseSalary, 18, 2);
    }
}

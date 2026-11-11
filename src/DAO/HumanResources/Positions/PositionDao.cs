using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using DAO.Infrastructure;
using DTO.HumanResources.Positions;
namespace DAO.HumanResources.Positions;
public sealed class PositionDao(MyConnection connectionFactory) : IPositionDao {
    public async Task<List<PositionDto>> GetAllAsync() {
        var items = new List<PositionDto>();
        using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Code, Name FROM dbo.Positions ORDER BY Code ASC;";
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync()) {
            items.Add(new PositionDto {
                Code = reader.GetString(reader.GetOrdinal("Code")),
                Name = reader.GetString(reader.GetOrdinal("Name"))
            });
        }
        return items;
    }
    public async Task<bool> ExistsAsync(string code) {
        using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(1) FROM dbo.Positions WHERE Code = @Code;";
        command.Parameters.Add("@Code", SqlDbType.VarChar, 20).Value = code;
        return Convert.ToInt32(await command.ExecuteScalarAsync()) > 0;
    }
    public async Task InsertAsync(PositionDto item) {
        using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO dbo.Positions (Code, Name) VALUES (@Code, @Name);";
        command.Parameters.Add("@Code", SqlDbType.VarChar, 20).Value = item.Code;
        command.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = item.Name;
        try { await command.ExecuteNonQueryAsync(); }
        catch (SqlException ex) { throw HrSqlParameterHelper.CreateWriteException(ex); }
    }
    public async Task UpdateAsync(PositionDto item) {
        using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandText = "UPDATE dbo.Positions SET Name = @Name WHERE Code = @Code;";
        command.Parameters.Add("@Code", SqlDbType.VarChar, 20).Value = item.Code;
        command.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = item.Name;
        try { if (await command.ExecuteNonQueryAsync() == 0) throw new InvalidOperationException("Mã chức vụ không tồn tại."); }
        catch (SqlException ex) { throw HrSqlParameterHelper.CreateWriteException(ex); }
    }
    public async Task DeleteAsync(string code) {
        using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM dbo.Positions WHERE Code = @Code;";
        command.Parameters.Add("@Code", SqlDbType.VarChar, 20).Value = code;
        try { if (await command.ExecuteNonQueryAsync() == 0) throw new InvalidOperationException("Mã chức vụ không tồn tại."); }
        catch (SqlException ex) when (ex.Number == 547) { throw new InvalidOperationException("Không thể xóa vì còn dữ liệu liên quan.", ex); }
    }
}

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
        command.CommandText = "SELECT Id, Code, Name FROM dbo.Positions ORDER BY Id DESC;";
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync()) {
            items.Add(new PositionDto {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Code = reader.GetString(reader.GetOrdinal("Code")),
                Name = reader.GetString(reader.GetOrdinal("Name"))
            });
        }
        return items;
    }
    public async Task<int> InsertAsync(PositionDto item) {
        using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO dbo.Positions (Code, Name) OUTPUT INSERTED.Id VALUES (@Code, @Name);";
        command.Parameters.Add("@Code", SqlDbType.VarChar, 20).Value = item.Code;
        command.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = item.Name;
        try { return Convert.ToInt32(await command.ExecuteScalarAsync()); }
        catch (SqlException ex) { throw HrSqlParameterHelper.CreateWriteException(ex); }
    }
    public async Task UpdateAsync(PositionDto item) {
        using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandText = "UPDATE dbo.Positions SET Code = @Code, Name = @Name WHERE Id = @Id;";
        command.Parameters.Add("@Id", SqlDbType.Int).Value = item.Id;
        command.Parameters.Add("@Code", SqlDbType.VarChar, 20).Value = item.Code;
        command.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = item.Name;
        try { if (await command.ExecuteNonQueryAsync() == 0) throw new InvalidOperationException("Bản ghi không tồn tại."); }
        catch (SqlException ex) { throw HrSqlParameterHelper.CreateWriteException(ex); }
    }
    public async Task DeleteAsync(int id) {
        using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM dbo.Positions WHERE Id = @Id;";
        command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
        try { if (await command.ExecuteNonQueryAsync() == 0) throw new InvalidOperationException("Bản ghi không tồn tại."); }
        catch (SqlException ex) when (ex.Number == 547) { throw new InvalidOperationException("Không thể xóa vì còn dữ liệu liên quan.", ex); }
    }
}

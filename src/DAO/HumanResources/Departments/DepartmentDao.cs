using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using DAO.Infrastructure;
using DTO.HumanResources.Departments;
using Microsoft.Data.SqlClient;

namespace DAO.HumanResources.Departments;

public sealed class DepartmentDao(MyConnection connection_factory) : IDepartmentDao
{
    public async Task<List<DepartmentDto>> GetAllAsync(CancellationToken cancellation_token = default)
    {
        var items = new List<DepartmentDto>();
        using var connection = connection_factory.CreateConnection();
        await connection.OpenAsync(cancellation_token);
        await HrSqlParameterHelper.EnsureSchemaAsync(connection, cancellation_token);

        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, Code, Name, Description
            FROM dbo.Departments
            ORDER BY Code ASC;
            """;

        using var reader = await command.ExecuteReaderAsync(cancellation_token);
        int id_ordinal = reader.GetOrdinal("Id");
        int code_ordinal = reader.GetOrdinal("Code");
        int name_ordinal = reader.GetOrdinal("Name");
        int description_ordinal = reader.GetOrdinal("Description");

        while (await reader.ReadAsync(cancellation_token))
        {
            items.Add(new DepartmentDto
            {
                Id = reader.GetInt32(id_ordinal),
                Code = reader.GetString(code_ordinal),
                Name = reader.GetString(name_ordinal),
                Description = reader.IsDBNull(description_ordinal) ? string.Empty : reader.GetString(description_ordinal)
            });
        }

        return items;
    }

    public async Task<bool> ExistsAsync(string code, CancellationToken cancellation_token = default)
    {
        using var connection = connection_factory.CreateConnection();
        await connection.OpenAsync(cancellation_token);
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(1) FROM dbo.Departments WHERE Code = @Code;";
        command.Parameters.Add("@Code", SqlDbType.VarChar, 20).Value = code;
        return Convert.ToInt32(await command.ExecuteScalarAsync(cancellation_token)) > 0;
    }

    public async Task<int> InsertAsync(DepartmentDto item, CancellationToken cancellation_token = default)
    {
        using var connection = connection_factory.CreateConnection();
        await connection.OpenAsync(cancellation_token);
        await HrSqlParameterHelper.EnsureSchemaAsync(connection, cancellation_token);

        using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO dbo.Departments (Code, Name, Description)
            OUTPUT INSERTED.Id
            VALUES (@Code, @Name, @Description);
            """;
        AddParameters(command, item);

        try
        {
            int inserted_id = Convert.ToInt32(await command.ExecuteScalarAsync(cancellation_token));
            item.Id = inserted_id;
            return inserted_id;
        }
        catch (SqlException exception)
        {
            throw HrSqlParameterHelper.CreateWriteException(exception);
        }
    }

    public async Task UpdateAsync(DepartmentDto item, CancellationToken cancellation_token = default)
    {
        using var connection = connection_factory.CreateConnection();
        await connection.OpenAsync(cancellation_token);
        await HrSqlParameterHelper.EnsureSchemaAsync(connection, cancellation_token);

        using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE dbo.Departments
            SET Name = @Name, Description = @Description
            WHERE Code = @Code OR (@Id > 0 AND Id = @Id);
            """;
        command.Parameters.Add("@Id", SqlDbType.Int).Value = item.Id;
        AddParameters(command, item);

        try
        {
            if (await command.ExecuteNonQueryAsync(cancellation_token) == 0)
                throw new InvalidOperationException("Mã phòng ban không tồn tại.");
        }
        catch (SqlException exception)
        {
            throw HrSqlParameterHelper.CreateWriteException(exception);
        }
    }

    public async Task DeleteAsync(string code, CancellationToken cancellation_token = default)
    {
        using var connection = connection_factory.CreateConnection();
        await connection.OpenAsync(cancellation_token);
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM dbo.Departments WHERE Code = @Code;";
        command.Parameters.Add("@Code", SqlDbType.VarChar, 20).Value = code;

        try
        {
            if (await command.ExecuteNonQueryAsync(cancellation_token) == 0)
                throw new InvalidOperationException("Mã phòng ban không tồn tại.");
        }
        catch (SqlException exception) when (exception.Number == 547)
        {
            throw new InvalidOperationException("Không thể xóa vì phòng ban đang được sử dụng bởi nhân viên hoặc dự án.", exception);
        }
    }

    private static void AddParameters(SqlCommand command, DepartmentDto item)
    {
        command.Parameters.Add("@Code", SqlDbType.VarChar, 20).Value = item.Code;
        command.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = item.Name;
        command.Parameters.Add("@Description", SqlDbType.NVarChar, 500).Value = item.Description;
    }
}

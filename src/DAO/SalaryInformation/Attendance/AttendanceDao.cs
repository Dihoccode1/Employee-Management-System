using System.Data;
using DAO.Infrastructure;
using DTO.SalaryInformation.Attendance;
using DTO.SalaryInformation.AttendanceStatuses;
using Microsoft.Data.SqlClient;

namespace DAO.SalaryInformation.Attendance;

public sealed class AttendanceDao(MyConnection connection_factory)
{
    public async Task<List<AttendanceStatusDto>> GetStatusesAsync()
    {
        using var connection = connection_factory.CreateConnection();
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, AttendanceStatus, Description
            FROM dbo.AttendanceStatuses
            ORDER BY AttendanceStatus, Id;
            """;

        var statuses = new List<AttendanceStatusDto>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            statuses.Add(new AttendanceStatusDto
            {
                Id = reader.GetInt32(0),
                AttendanceStatus = reader.GetString(1),
                Description = reader.GetString(2)
            });
        }

        return statuses;
    }

    public async Task<List<AttendanceDto>> GetRecordsAsync()
    {
        using var connection = connection_factory.CreateConnection();
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT a.Id, a.EmployeeId, a.AttendanceStatusId, a.WorkDate,
                   a.CheckIn, a.CheckOut, a.AttendanceStatus, e.EmployeeCode, e.FullName
            FROM dbo.Attendance AS a
            JOIN dbo.Employees AS e ON e.Id = a.EmployeeId
            ORDER BY a.WorkDate DESC, a.Id DESC;
            """;

        var records = new List<AttendanceDto>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            records.Add(new AttendanceDto
            {
                Id = reader.GetInt32(0),
                EmployeeId = reader.GetInt32(1),
                AttendanceStatusId = reader.GetInt32(2),
                WorkDate = reader.GetDateTime(3),
                CheckIn = reader.GetTimeSpan(4),
                CheckOut = reader.GetTimeSpan(5),
                AttendanceStatus = reader.GetString(6),
                EmployeeCode = reader.GetString(7),
                EmployeeName = reader.GetString(8)
            });
        }

        return records;
    }

    public async Task<int> SaveStatusAsync(int status_id, string status_name, string description)
    {
        using var connection = connection_factory.CreateConnection();
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandText = status_id == 0
            ? """
                INSERT dbo.AttendanceStatuses (AttendanceStatus, Description)
                OUTPUT INSERTED.Id
                VALUES (@Name, @Description);
                """
            : """
                UPDATE dbo.AttendanceStatuses
                SET AttendanceStatus = @Name, Description = @Description
                OUTPUT INSERTED.Id
                WHERE Id = @Id;
                """;

        command.Parameters.Add("@Id", SqlDbType.Int).Value = status_id;
        command.Parameters.Add("@Name", SqlDbType.NVarChar, 50).Value = status_name;
        command.Parameters.Add("@Description", SqlDbType.NVarChar, 500).Value = description;
        return await SaveAsync(command);
    }

    public async Task<int> SaveRecordAsync(AttendanceDto record)
    {
        using var connection = connection_factory.CreateConnection();
        await connection.OpenAsync();
        using var command = connection.CreateCommand();

        // Lấy tên trạng thái từ bảng cha ngay khi lưu để mã và tên khớp nhau.
        command.CommandText = record.Id == 0
            ? """
                INSERT dbo.Attendance
                    (EmployeeId, AttendanceStatusId, WorkDate, CheckIn, CheckOut, AttendanceStatus)
                OUTPUT INSERTED.Id
                SELECT @EmployeeId, s.Id, @WorkDate, @CheckIn, @CheckOut, s.AttendanceStatus
                FROM dbo.AttendanceStatuses AS s
                WHERE s.Id = @StatusId;
                """
            : """
                UPDATE a
                SET EmployeeId = @EmployeeId, AttendanceStatusId = s.Id, WorkDate = @WorkDate,
                    CheckIn = @CheckIn, CheckOut = @CheckOut, AttendanceStatus = s.AttendanceStatus
                OUTPUT INSERTED.Id
                FROM dbo.Attendance AS a
                JOIN dbo.AttendanceStatuses AS s ON s.Id = @StatusId
                WHERE a.Id = @Id;
                """;

        command.Parameters.Add("@Id", SqlDbType.Int).Value = record.Id;
        command.Parameters.Add("@EmployeeId", SqlDbType.Int).Value = record.EmployeeId;
        command.Parameters.Add("@StatusId", SqlDbType.Int).Value = record.AttendanceStatusId;
        command.Parameters.Add("@WorkDate", SqlDbType.Date).Value = record.WorkDate.Date;
        command.Parameters.Add("@CheckIn", SqlDbType.Time).Value = record.CheckIn;
        command.Parameters.Add("@CheckOut", SqlDbType.Time).Value = record.CheckOut;
        return await SaveAsync(command);
    }

    public Task DeleteStatusAsync(int status_id) =>
        DeleteAsync("DELETE FROM dbo.AttendanceStatuses WHERE Id = @Id;", status_id);

    public Task DeleteRecordAsync(int record_id) =>
        DeleteAsync("DELETE FROM dbo.Attendance WHERE Id = @Id;", record_id);

    private static async Task<int> SaveAsync(SqlCommand command)
    {
        try
        {
            if (await command.ExecuteScalarAsync() is int saved_id)
                return saved_id;

            throw new InvalidOperationException("Bản ghi hoặc trạng thái đã thay đổi. Hãy tải lại danh sách.");
        }
        catch (SqlException exception)
        {
            throw SqlParameterHelper.CreateWriteException(exception);
        }
    }

    private async Task DeleteAsync(string sql, int id)
    {
        using var connection = connection_factory.CreateConnection();
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.Add("@Id", SqlDbType.Int).Value = id;

        try
        {
            if (await command.ExecuteNonQueryAsync() == 0)
                throw new InvalidOperationException("Bản ghi đã bị xóa. Hãy tải lại danh sách.");
        }
        catch (SqlException exception) when (exception.Number == 547)
        {
            // SQL Server bảo vệ các bản ghi đang được bảng khác tham chiếu.
            throw new InvalidOperationException("Không thể xóa vì vẫn còn dữ liệu liên quan.", exception);
        }
    }
}

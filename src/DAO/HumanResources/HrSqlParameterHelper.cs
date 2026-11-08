using System;
using System.Data;
using Microsoft.Data.SqlClient;
namespace DAO.HumanResources;
internal static class HrSqlParameterHelper {
    public static void AddDecimal(SqlCommand command, string name, decimal value, byte precision = 18, byte scale = 2) {
        SqlParameter parameter = command.Parameters.Add(name, SqlDbType.Decimal);
        parameter.Precision = precision; parameter.Scale = scale; parameter.Value = value;
    }
    public static Exception CreateWriteException(SqlException exception) => exception.Number switch {
        2601 or 2627 => new InvalidOperationException("Mã bản ghi đã tồn tại trong hệ thống.", exception),
        547 => new InvalidOperationException("Dữ liệu tham chiếu hoặc ràng buộc không hợp lệ.", exception),
        _ => new InvalidOperationException("Không thể lưu dữ liệu vào cơ sở dữ liệu.", exception)
    };
}

namespace DTO.SalaryInformation.Attendance;

public sealed class AttendanceDto
{
    public int Id { get; init; }
    public int EmployeeId { get; init; }
    public int AttendanceStatusId { get; init; }
    public DateTime WorkDate { get; init; }
    public TimeSpan CheckIn { get; init; }
    public TimeSpan CheckOut { get; init; }
    public string AttendanceStatus { get; init; } = string.Empty;

    // Hai giá trị từ bảng Employees dùng để hiển thị và tìm kiếm.
    public string EmployeeCode { get; init; } = string.Empty;
    public string EmployeeName { get; init; } = string.Empty;
}

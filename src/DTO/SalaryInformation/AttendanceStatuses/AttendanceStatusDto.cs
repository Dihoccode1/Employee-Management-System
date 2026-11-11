namespace DTO.SalaryInformation.AttendanceStatuses;

public sealed class AttendanceStatusDto
{
    public int Id { get; init; }
    public string AttendanceStatus { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}

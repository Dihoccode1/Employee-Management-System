using System.Collections;
using DAO.SalaryInformation.Attendance;
using DTO.SalaryInformation.Attendance;
using DTO.SalaryInformation.AttendanceStatuses;

namespace BUS.SalaryInformation.Attendance;

public sealed class AttendanceService(AttendanceDao attendance_dao)
{
    public Task<List<AttendanceStatusDto>> GetStatusesAsync() => attendance_dao.GetStatusesAsync();

    public Task<List<AttendanceDto>> GetRecordsAsync() => attendance_dao.GetRecordsAsync();

    public Task<int> SaveStatusAsync(AttendanceStatusDto status)
    {
        if (status.Id < 0)
            throw new ArgumentException("Mã trạng thái không hợp lệ.");

        string status_name = status.AttendanceStatus.Trim();
        string description = status.Description.Trim();
        if (status_name.Length is 0 or > 50)
            throw new ArgumentException("Tên trạng thái phải có từ 1 đến 50 ký tự.");
        if (description.Length > 500)
            throw new ArgumentException("Mô tả tối đa 500 ký tự.");

        return attendance_dao.SaveStatusAsync(status.Id, status_name, description);
    }

    public Task<int> SaveRecordAsync(AttendanceDto record)
    {
        if (record.Id < 0 || record.EmployeeId <= 0 || record.AttendanceStatusId <= 0)
            throw new ArgumentException("Hãy chọn nhân viên và trạng thái chấm công.");
        if (record.WorkDate == default)
            throw new ArgumentException("Ngày chấm công không hợp lệ.");

        // Kiểu TIME của SQL Server chỉ lưu thời điểm trong một ngày.
        if (record.CheckIn < TimeSpan.Zero || record.CheckIn >= TimeSpan.FromDays(1) ||
            record.CheckOut < TimeSpan.Zero || record.CheckOut >= TimeSpan.FromDays(1))
            throw new ArgumentException("Giờ vào và giờ ra phải nằm trong ngày.");

        return attendance_dao.SaveRecordAsync(record);
    }

    public Task DeleteStatusAsync(int status_id)
    {
        if (status_id <= 0)
            throw new ArgumentException("Hãy chọn trạng thái cần xóa.");

        return attendance_dao.DeleteStatusAsync(status_id);
    }

    public Task DeleteRecordAsync(int record_id)
    {
        if (record_id <= 0)
            throw new ArgumentException("Hãy chọn bản ghi chấm công cần xóa.");

        return attendance_dao.DeleteRecordAsync(record_id);
    }

    // Lọc danh sách đã tải theo yêu cầu tìm kiếm trên ArrayList của môn học.
    public List<AttendanceStatusDto> SearchStatuses(ArrayList source, string keyword)
    {
        string term = keyword.Trim();
        return source.Cast<AttendanceStatusDto>()
            .Where(status => status.AttendanceStatus.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                             status.Description.Contains(term, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public List<AttendanceDto> SearchRecords(ArrayList source, int status_id, string keyword)
    {
        string term = keyword.Trim();
        return source.Cast<AttendanceDto>()
            .Where(record => record.AttendanceStatusId == status_id)
            .Where(record => record.EmployeeCode.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                             record.EmployeeName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                             record.WorkDate.ToString("dd/MM/yyyy").Contains(term, StringComparison.Ordinal))
            .ToList();
    }
}

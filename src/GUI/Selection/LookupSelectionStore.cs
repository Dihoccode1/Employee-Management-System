using DTO.HumanResources.Departments;
using DTO.HumanResources.Employees;
using DTO.HumanResources.Positions;

namespace GUI.Selection;

/// <summary>
/// Lưu trữ kết quả lựa chọn từ các form tra cứu thông qua biến static (lấy một lần rồi xóa để tránh dùng lại dữ liệu cũ).
/// </summary>
public static class LookupSelectionStore
{
    private static EmployeeDto? _selected_employee;
    private static DepartmentDto? _selected_department;
    private static PositionDto? _selected_position;

    public static void SetEmployee(EmployeeDto? employee)
    {
        _selected_employee = employee;
    }

    public static EmployeeDto? TakeEmployee()
    {
        EmployeeDto? item = _selected_employee;
        _selected_employee = null;
        return item;
    }

    public static void SetDepartment(DepartmentDto? department)
    {
        _selected_department = department;
    }

    public static DepartmentDto? TakeDepartment()
    {
        DepartmentDto? item = _selected_department;
        _selected_department = null;
        return item;
    }

    public static void SetPosition(PositionDto? position)
    {
        _selected_position = position;
    }

    public static PositionDto? TakePosition()
    {
        PositionDto? item = _selected_position;
        _selected_position = null;
        return item;
    }
}

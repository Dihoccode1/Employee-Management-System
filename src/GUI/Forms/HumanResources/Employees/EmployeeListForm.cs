using BUS.HumanResources.Employees;
using DTO.HumanResources.Employees;

namespace GUI.Forms.HumanResources.Employees;

internal sealed class EmployeeListForm : Form
{
    private readonly EmployeeService _employee_service;
    private readonly Button _load_button = new() { Text = "Tải danh sách nhân viên", Dock = DockStyle.Top, Height = 44 };
    private readonly DataGridView _employees_grid = new()
    {
        Dock = DockStyle.Fill,
        ReadOnly = true,
        AllowUserToAddRows = false,
        AllowUserToDeleteRows = false,
        AutoGenerateColumns = false,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells
    };

    public EmployeeListForm(EmployeeService employee_service)
    {
        _employee_service = employee_service;
        Text = "Danh sách nhân viên";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(1180, 620);

        AddColumn(nameof(EmployeeDto.Id), "ID");
        AddColumn(nameof(EmployeeDto.EmployeeCode), "Mã NV");
        AddColumn(nameof(EmployeeDto.FullName), "Họ và tên");
        AddColumn(nameof(EmployeeDto.DateOfBirth), "Ngày sinh", "dd/MM/yyyy");
        AddColumn(nameof(EmployeeDto.Gender), "Giới tính");
        AddColumn(nameof(EmployeeDto.PhoneNumber), "Điện thoại");
        AddColumn(nameof(EmployeeDto.Email), "Email");
        AddColumn(nameof(EmployeeDto.DepartmentName), "Phòng ban");
        AddColumn(nameof(EmployeeDto.PositionName), "Chức vụ");
        AddColumn(nameof(EmployeeDto.HireDate), "Ngày vào làm", "dd/MM/yyyy");
        AddColumn(nameof(EmployeeDto.CurrentBaseSalary), "Lương CB", "N0");
        AddColumn(nameof(EmployeeDto.Status), "Trạng thái");
        AddColumn(nameof(EmployeeDto.Degree), "Bằng cấp");

        Controls.Add(_employees_grid);
        Controls.Add(_load_button);
        _load_button.Click += async (_, _) => await LoadEmployeesAsync();
        Shown += async (_, _) => await LoadEmployeesAsync();
    }

    private void AddColumn(string property_name, string header_text, string? format = null)
    {
        var column = new DataGridViewTextBoxColumn
        {
            DataPropertyName = property_name,
            HeaderText = header_text
        };
        if (format is not null)
            column.DefaultCellStyle.Format = format;
        _employees_grid.Columns.Add(column);
    }

    private async Task LoadEmployeesAsync()
    {
        _load_button.Enabled = false;
        try
        {
            var employees = await _employee_service.GetAllAsync();
            if (!IsDisposed)
                _employees_grid.DataSource = employees;
        }
        catch (Exception) when (IsDisposed)
        {
        }
        catch (Exception)
        {
            MessageBox.Show(this, "Không thể tải dữ liệu. Kiểm tra kết nối và script SQL Server.",
                "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            if (!IsDisposed)
                _load_button.Enabled = true;
        }
    }
}

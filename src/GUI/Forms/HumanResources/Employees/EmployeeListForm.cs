using BUS.HumanResources.Employees;

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
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells
    };

    public EmployeeListForm(EmployeeService employee_service)
    {
        _employee_service = employee_service;
        Text = "Danh sách nhân viên";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(1000, 560);
        Controls.Add(_employees_grid);
        Controls.Add(_load_button);
        _load_button.Click += async (_, _) => await LoadEmployeesAsync();
        Shown += async (_, _) => await LoadEmployeesAsync();
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

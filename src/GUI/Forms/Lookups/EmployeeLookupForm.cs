using System.Collections;
using BUS.HumanResources.Employees;
using DTO.HumanResources.Employees;
using GUI.Selection;

namespace GUI.Forms.Lookups;

public sealed class EmployeeLookupForm : Form
{
    private readonly EmployeeService _employee_service;
    private readonly ArrayList _employees;
    private readonly TextBox _search_box = new() { Dock = DockStyle.Top, PlaceholderText = "Tìm theo mã hoặc họ tên" };
    private readonly ListView _employee_list = new()
    {
        Dock = DockStyle.Fill,
        View = View.Details,
        FullRowSelect = true,
        MultiSelect = false
    };

    public EmployeeLookupForm(EmployeeService employee_service, List<EmployeeDto> employees)
    {
        _employee_service = employee_service;
        _employees = new ArrayList(employees);
        // Xóa lựa chọn cũ mỗi lần mở lại form tra cứu.
        LookupSelectionStore.SetEmployee(null);

        Text = "Chọn nhân viên";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(580, 380);
        _employee_list.Columns.Add("Mã nhân viên", 140);
        _employee_list.Columns.Add("Họ tên", 390);

        var select_button = new Button { Text = "Chọn", Dock = DockStyle.Bottom, Height = 36 };
        select_button.Click += (_, _) => ConfirmSelection();
        _employee_list.DoubleClick += (_, _) => ConfirmSelection();
        _search_box.TextChanged += (_, _) => RenderEmployees();
        AcceptButton = select_button;

        Controls.Add(_employee_list);
        Controls.Add(_search_box);
        Controls.Add(select_button);
        RenderEmployees();
    }

    // Form gọi lấy kết quả một lần qua LookupSelectionStore (static). Lần mở tiếp theo không dùng lại lựa chọn cũ.
    public static EmployeeDto? TakeSelection() => LookupSelectionStore.TakeEmployee();

    private void ConfirmSelection()
    {
        if (_employee_list.SelectedItems.Count != 1)
            return;

        LookupSelectionStore.SetEmployee((EmployeeDto)_employee_list.SelectedItems[0].Tag!);
        DialogResult = DialogResult.OK;
    }

    private void RenderEmployees()
    {
        _employee_list.BeginUpdate();
        try
        {
            _employee_list.Items.Clear();
            foreach (EmployeeDto employee in _employee_service.Search(_employees, _search_box.Text))
            {
                _employee_list.Items.Add(new ListViewItem([employee.EmployeeCode, employee.FullName])
                {
                    Tag = employee
                });
            }
        }
        finally
        {
            _employee_list.EndUpdate();
        }
    }
}

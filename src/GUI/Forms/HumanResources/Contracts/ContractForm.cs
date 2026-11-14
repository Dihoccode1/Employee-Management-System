using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Forms;
using BUS;
using BUS.HumanResources;
using BUS.HumanResources.Contracts;
using BUS.HumanResources.Employees;
using BUS.Settings;
using DTO.HumanResources.Contracts;
using DTO.HumanResources.Employees;

namespace GUI.Forms.HumanResources.Contracts;

/// <summary>
/// Form Main–Sub hai ListView: ListView trên hiển thị Nhân viên, ListView dưới hiển thị Hợp đồng
/// của nhân viên đang chọn (lấy dữ liệu qua BUS, cập nhật ngay khi chọn/bỏ chọn/đổi lựa chọn).
/// </summary>
public sealed class ContractForm : Form
{
    private readonly ContractService _contract_service;
    private readonly EmployeeService _employee_service;
    private readonly SystemParameterService _parameter_service;
    private readonly string? _initial_employee_code;

    private ArrayList _employees = new();
    private ArrayList _contracts = new();
    private int _selected_employee_id;
    private string _selected_employee_code = string.Empty;
    private string _selected_employee_name = string.Empty;
    private int _editing_contract_id;
    private bool _is_edit_mode;
    private bool _rendering;
    private decimal _min_base_salary = 4960000m;

    private readonly TableLayoutPanel _layout = new() { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1 };
    private readonly ListView _employee_list = CreateListView();
    private readonly ListView _contract_list = CreateListView();

    private readonly TextBox _employee_search = new() { Width = 240, PlaceholderText = "Tìm nhân viên theo mã, họ tên, phòng ban..." };
    private readonly TextBox _contract_search = new() { Width = 220, PlaceholderText = "Lọc hợp đồng theo mã, loại, trạng thái..." };
    private readonly Label _selected_employee_label = new()
    {
        AutoSize = true,
        Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
        ForeColor = Color.FromArgb(24, 90, 140),
        Margin = new Padding(8, 7, 0, 0),
        Text = "Chưa chọn nhân viên (hãy chọn 1 nhân viên ở danh sách trên)"
    };
    private readonly Label _mode_label = new()
    {
        AutoSize = true,
        Font = new Font("Segoe UI", 9f, FontStyle.Bold),
        ForeColor = Color.FromArgb(40, 110, 60),
        Margin = new Padding(8, 7, 0, 0),
        Text = "Chế độ: Thêm hợp đồng mới"
    };

    private readonly TextBox _txt_contract_code = new() { Width = 140, MaxLength = 30 };
    private readonly TextBox _txt_employee_display = new() { Width = 210, ReadOnly = true };
    private readonly ComboBox _cmb_contract_type = new() { Width = 165, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly DateTimePicker _dtp_start = new() { Width = 120, Format = DateTimePickerFormat.Short };
    private readonly DateTimePicker _dtp_end = new() { Width = 135, Format = DateTimePickerFormat.Short, ShowCheckBox = true };
    private readonly TextBox _txt_salary = new() { Width = 130, PlaceholderText = "0" };
    private readonly ComboBox _cmb_status = new() { Width = 135, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly Label _salary_param_label = new()
    {
        AutoSize = true,
        Margin = new Padding(8, 7, 0, 0),
        ForeColor = Color.DimGray,
        Text = "Lương CB tối thiểu: đang tải..."
    };

    private readonly Button _btn_reload = new() { Text = "Tải lại", AutoSize = true };
    private readonly Button _btn_new = new() { Text = "Làm mới", AutoSize = true };
    private readonly Button _btn_add = new() { Text = "Thêm hợp đồng", AutoSize = true };
    private readonly Button _btn_update = new() { Text = "Cập nhật HĐ", AutoSize = true };
    private readonly Button _btn_delete = new() { Text = "Xóa HĐ", AutoSize = true };

    public ContractForm(string? connection_string = null, string? initial_employee_code = null)
    {
        string conn = connection_string
                      ?? Environment.GetEnvironmentVariable("HRM_CONNECTION_STRING")
                      ?? "Server=localhost;Database=HumanResourceManagement;Integrated Security=True;TrustServerCertificate=True";
        _contract_service = HumanResourcesServiceFactory.CreateContractService(conn);
        _employee_service = HumanResourcesServiceFactory.CreateEmployeeService(conn);
        _parameter_service = ServiceFactory.CreateSystemParameterService(conn);
        _initial_employee_code = initial_employee_code?.Trim();

        Text = "Quản lý Hợp đồng lao động (Main–Sub Nhân viên & Hợp đồng)";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(1180, 760);
        MinimumSize = new Size(980, 640);
        Font = new Font("Segoe UI", 9.5f);

        _cmb_contract_type.Items.AddRange(["Xác định thời hạn", "Không xác định thời hạn", "Thử việc", "Thời vụ"]);
        _cmb_contract_type.SelectedIndex = 0;

        _cmb_status.Items.AddRange(["Còn hiệu lực", "Hết hiệu lực", "Đã thanh lý", "Tạm hoãn"]);
        _cmb_status.SelectedIndex = 0;

        BuildLayout();
        HookEvents();
        SwitchContractToAddMode();
        Shown += async (_, _) => await ReloadAsync(_initial_employee_code);
    }

    private void BuildLayout()
    {
        _employee_list.Columns.Add("Mã NV", 95);
        _employee_list.Columns.Add("Họ tên nhân viên", 230);
        _employee_list.Columns.Add("Phòng ban", 190);
        _employee_list.Columns.Add("Chức vụ", 160);
        _employee_list.Columns.Add("Ngày vào làm", 110);
        _employee_list.Columns.Add("Lương CB hiện hành", 150);
        _employee_list.Columns.Add("Trạng thái", 130);

        _contract_list.Columns.Add("Mã HĐ", 115);
        _contract_list.Columns.Add("Mã NV", 95);
        _contract_list.Columns.Add("Họ tên nhân viên", 210);
        _contract_list.Columns.Add("Loại hợp đồng", 170);
        _contract_list.Columns.Add("Ngày bắt đầu", 110);
        _contract_list.Columns.Add("Ngày kết thúc", 110);
        _contract_list.Columns.Add("Lương cơ bản", 135);
        _contract_list.Columns.Add("Trạng thái", 130);

        var main_toolbar = CreateRow();
        main_toolbar.Controls.Add(_employee_search);
        main_toolbar.Controls.Add(_btn_reload);
        main_toolbar.Controls.Add(_selected_employee_label);

        var main_group = new GroupBox
        {
            Text = "Danh sách chính (Main) — Chọn nhân viên để xem hợp đồng",
            Dock = DockStyle.Fill,
            Padding = new Padding(8)
        };
        var main_panel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
        main_panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        main_panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        main_panel.Controls.Add(main_toolbar, 0, 0);
        main_panel.Controls.Add(_employee_list, 0, 1);
        main_group.Controls.Add(main_panel);

        var sub_actions = CreateRow();
        sub_actions.Controls.Add(_contract_search);
        sub_actions.Controls.Add(_btn_new);
        sub_actions.Controls.Add(_btn_add);
        sub_actions.Controls.Add(_btn_update);
        sub_actions.Controls.Add(_btn_delete);
        sub_actions.Controls.Add(_mode_label);

        var sub_fields = CreateRow();
        AddField(sub_fields, "Mã HĐ:", _txt_contract_code);
        AddField(sub_fields, "Nhân viên:", _txt_employee_display);
        AddField(sub_fields, "Loại HĐ:", _cmb_contract_type);
        AddField(sub_fields, "Ngày BĐ:", _dtp_start);
        AddField(sub_fields, "Ngày KT:", _dtp_end);
        AddField(sub_fields, "Lương CB:", _txt_salary);
        AddField(sub_fields, "Trạng thái:", _cmb_status);
        sub_fields.Controls.Add(_salary_param_label);

        var sub_group = new GroupBox
        {
            Text = "Danh sách con (Sub) — Hợp đồng lao động của nhân viên đang chọn",
            Dock = DockStyle.Fill,
            Padding = new Padding(8)
        };
        var sub_panel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
        sub_panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        sub_panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        sub_panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        sub_panel.Controls.Add(sub_actions, 0, 0);
        sub_panel.Controls.Add(sub_fields, 0, 1);
        sub_panel.Controls.Add(_contract_list, 0, 2);
        sub_group.Controls.Add(sub_panel);

        _layout.RowStyles.Add(new RowStyle(SizeType.Percent, 46));
        _layout.RowStyles.Add(new RowStyle(SizeType.Percent, 54));
        _layout.Controls.Add(main_group, 0, 0);
        _layout.Controls.Add(sub_group, 0, 1);
        Controls.Add(_layout);
    }

    private void HookEvents()
    {
        _btn_reload.Click += async (_, _) => await ReloadAsync(_selected_employee_code);
        _employee_search.TextChanged += (_, _) => RenderEmployees(_selected_employee_code);
        _contract_search.TextChanged += (_, _) => RenderContractsForSelectedEmployee();

        // Cập nhật ListView con ngay khi người dùng chọn, đổi hoặc bỏ chọn nhân viên ở ListView chính.
        _employee_list.SelectedIndexChanged += (_, _) =>
        {
            if (!_rendering)
                OnSelectedEmployeeChanged();
        };

        _contract_list.SelectedIndexChanged += (_, _) =>
        {
            if (!_rendering)
                ShowSelectedContract();
        };

        _btn_new.Click += (_, _) => SwitchContractToAddMode();
        _btn_add.Click += async (_, _) => await SaveContractAsync(is_edit_mode: false);
        _btn_update.Click += async (_, _) => await SaveContractAsync(is_edit_mode: true);
        _btn_delete.Click += async (_, _) => await DeleteContractAsync();
    }

    private async Task ReloadAsync(string? preferred_employee_code = null, string? preferred_contract_code = null)
    {
        try
        {
            _min_base_salary = await _parameter_service.GetDecimalValueAsync("MIN_BASE_SALARY", 4960000m);
            _salary_param_label.Text = $"(Lương CB tối thiểu theo tham số: {_min_base_salary:N0} VNĐ)";

            List<EmployeeDto> employees = await _employee_service.GetAllAsync();
            List<ContractDto> contracts = await _contract_service.GetAllAsync();
            _employees = new ArrayList(employees);
            _contracts = new ArrayList(contracts);

            RenderEmployees(preferred_employee_code, preferred_contract_code);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Lỗi tải dữ liệu hợp đồng", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void RenderEmployees(string? preferred_employee_code = null, string? preferred_contract_code = null)
    {
        _rendering = true;
        _employee_list.BeginUpdate();
        try
        {
            _employee_list.Items.Clear();
            ListViewItem? matched_item = null;
            foreach (EmployeeDto emp in _employee_service.Search(_employees, _employee_search.Text))
            {
                var item = new ListViewItem([
                    emp.EmployeeCode,
                    emp.FullName,
                    string.IsNullOrEmpty(emp.DepartmentCode) ? "-" : $"{emp.DepartmentCode} - {emp.DepartmentName}",
                    string.IsNullOrEmpty(emp.PositionCode) ? "-" : emp.PositionName,
                    emp.HireDate.ToString("dd/MM/yyyy"),
                    emp.CurrentBaseSalary.ToString("N0"),
                    emp.Status
                ])
                {
                    Tag = emp
                };
                _employee_list.Items.Add(item);

                if (!string.IsNullOrWhiteSpace(preferred_employee_code)
                    && string.Equals(emp.EmployeeCode, preferred_employee_code, StringComparison.OrdinalIgnoreCase))
                {
                    matched_item = item;
                }
            }

            if (matched_item is not null)
            {
                matched_item.Selected = true;
                matched_item.EnsureVisible();
            }
        }
        finally
        {
            _employee_list.EndUpdate();
            _rendering = false;
        }

        OnSelectedEmployeeChanged(preferred_contract_code);
    }

    private void OnSelectedEmployeeChanged(string? preferred_contract_code = null)
    {
        if (_employee_list.SelectedItems.Count != 1
            || _employee_list.SelectedItems[0].Tag is not EmployeeDto selected)
        {
            // Bỏ chọn ở danh sách chính phải xóa trống danh sách con ngay lập tức.
            _selected_employee_id = 0;
            _selected_employee_code = string.Empty;
            _selected_employee_name = string.Empty;
            _selected_employee_label.Text = "Chưa chọn nhân viên (danh sách hợp đồng đang trống)";
            _contract_list.Items.Clear();
            SwitchContractToAddMode();
            return;
        }

        _selected_employee_id = selected.Id;
        _selected_employee_code = selected.EmployeeCode;
        _selected_employee_name = selected.FullName;
        _selected_employee_label.Text = $"Đang chọn: {selected.EmployeeCode} · {selected.FullName}";
        SwitchContractToAddMode();
        RenderContractsForSelectedEmployee(preferred_contract_code);
    }

    private void RenderContractsForSelectedEmployee(string? preferred_contract_code = null)
    {
        _rendering = true;
        _contract_list.BeginUpdate();
        try
        {
            _contract_list.Items.Clear();
            if (_selected_employee_id <= 0 && string.IsNullOrWhiteSpace(_selected_employee_code))
                return;

            ArrayList filtered = _contract_service.SearchByEmployee(
                _contracts,
                _selected_employee_id,
                _selected_employee_code,
                _contract_search.Text);

            foreach (ContractDto contract in filtered)
            {
                var item = new ListViewItem([
                    contract.ContractCode,
                    contract.EmployeeCode,
                    contract.EmployeeName,
                    contract.ContractType,
                    contract.StartDate.ToString("dd/MM/yyyy"),
                    contract.EndDate?.ToString("dd/MM/yyyy") ?? "Không thời hạn",
                    contract.BaseSalary.ToString("N0"),
                    contract.Status
                ])
                {
                    Tag = contract
                };
                _contract_list.Items.Add(item);

                if (!string.IsNullOrWhiteSpace(preferred_contract_code)
                    && string.Equals(contract.ContractCode, preferred_contract_code, StringComparison.OrdinalIgnoreCase))
                {
                    item.Selected = true;
                }
            }
        }
        finally
        {
            _contract_list.EndUpdate();
            _rendering = false;
        }

        ShowSelectedContract();
    }

    private void SwitchContractToAddMode()
    {
        _is_edit_mode = false;
        _editing_contract_id = 0;
        _txt_contract_code.Clear();
        _txt_contract_code.ReadOnly = false;
        _txt_employee_display.Text = string.IsNullOrEmpty(_selected_employee_code)
            ? string.Empty
            : $"{_selected_employee_code} · {_selected_employee_name}";
        _cmb_contract_type.SelectedIndex = 0;
        _dtp_start.Value = DateTime.Today;
        _dtp_end.Checked = true;
        _dtp_end.Value = DateTime.Today.AddYears(1);
        _txt_salary.Text = _min_base_salary.ToString("0.##", CultureInfo.InvariantCulture);
        _cmb_status.SelectedIndex = 0;
        _contract_list.SelectedItems.Clear();
        _btn_add.Enabled = true;
        _btn_update.Enabled = false;
        _btn_delete.Enabled = false;
        _mode_label.Text = "Chế độ: Thêm hợp đồng mới";
    }

    private void ShowSelectedContract()
    {
        if (_contract_list.SelectedItems.Count != 1
            || _contract_list.SelectedItems[0].Tag is not ContractDto contract)
        {
            return;
        }

        _is_edit_mode = true;
        _editing_contract_id = contract.Id;
        _txt_contract_code.Text = contract.ContractCode;
        _txt_contract_code.ReadOnly = true;
        _txt_employee_display.Text = $"{contract.EmployeeCode} · {contract.EmployeeName}";
        EnsureComboSelection(_cmb_contract_type, contract.ContractType);
        _dtp_start.Value = contract.StartDate;
        if (contract.EndDate.HasValue)
        {
            _dtp_end.Checked = true;
            _dtp_end.Value = contract.EndDate.Value;
        }
        else
        {
            _dtp_end.Checked = false;
        }
        _txt_salary.Text = contract.BaseSalary.ToString("0.##", CultureInfo.InvariantCulture);
        EnsureComboSelection(_cmb_status, contract.Status);
        _btn_add.Enabled = false;
        _btn_update.Enabled = true;
        _btn_delete.Enabled = true;
        _mode_label.Text = $"Chế độ: Đang sửa HĐ [{contract.ContractCode}]";
    }

    private async Task SaveContractAsync(bool is_edit_mode)
    {
        try
        {
            if (_selected_employee_id <= 0 && string.IsNullOrWhiteSpace(_selected_employee_code))
                throw new ArgumentException("Vui lòng chọn một nhân viên ở danh sách trên trước khi lưu hợp đồng.");

            if (!decimal.TryParse(_txt_salary.Text.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out decimal salary)
                && !decimal.TryParse(_txt_salary.Text.Trim(), NumberStyles.Number, CultureInfo.CurrentCulture, out salary))
            {
                throw new ArgumentException("Lương cơ bản hợp đồng phải là số hợp lệ.");
            }

            var dto = new ContractDto
            {
                Id = is_edit_mode ? _editing_contract_id : 0,
                ContractCode = _txt_contract_code.Text,
                EmployeeId = _selected_employee_id,
                EmployeeCode = _selected_employee_code,
                EmployeeName = _selected_employee_name,
                ContractType = _cmb_contract_type.SelectedItem?.ToString() ?? "Xác định thời hạn",
                StartDate = _dtp_start.Value.Date,
                EndDate = _dtp_end.Checked ? _dtp_end.Value.Date : null,
                BaseSalary = salary,
                Status = _cmb_status.SelectedItem?.ToString() ?? "Còn hiệu lực"
            };

            string saved_code = await _contract_service.SaveAsync(dto, is_edit_mode);
            await ReloadAsync(_selected_employee_code, saved_code);
            MessageBox.Show(this, is_edit_mode ? "Đã cập nhật hợp đồng." : "Đã thêm hợp đồng mới.",
                "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            MessageBox.Show(this, ex.Message, "Kiểm tra hợp đồng", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private async Task DeleteContractAsync()
    {
        if (!_is_edit_mode || string.IsNullOrWhiteSpace(_txt_contract_code.Text))
            return;

        if (MessageBox.Show(this, $"Xóa hợp đồng '{_txt_contract_code.Text}'?", "Xác nhận xóa",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        try
        {
            await _contract_service.DeleteAsync(_txt_contract_code.Text);
            await ReloadAsync(_selected_employee_code);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            MessageBox.Show(this, ex.Message, "Không thể xóa", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private static void EnsureComboSelection(ComboBox combo_box, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return;

        int existing_index = combo_box.FindStringExact(value);
        if (existing_index >= 0)
        {
            combo_box.SelectedIndex = existing_index;
            return;
        }

        combo_box.Items.Add(value);
        combo_box.SelectedItem = value;
    }

    private static ListView CreateListView() => new()
    {
        Dock = DockStyle.Fill,
        View = View.Details,
        FullRowSelect = true,
        MultiSelect = false,
        HideSelection = false
    };

    private static FlowLayoutPanel CreateRow() => new()
    {
        Dock = DockStyle.Fill,
        AutoSize = true,
        Padding = new Padding(2)
    };

    private static void AddField(FlowLayoutPanel row, string caption, Control input)
    {
        row.Controls.Add(new Label { Text = caption, AutoSize = true, Margin = new Padding(6, 7, 3, 0) });
        row.Controls.Add(input);
    }
}

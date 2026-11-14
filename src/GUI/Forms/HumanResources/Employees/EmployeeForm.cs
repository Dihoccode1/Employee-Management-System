using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Forms;
using BUS;
using BUS.HumanResources;
using BUS.HumanResources.Departments;
using BUS.HumanResources.Employees;
using BUS.HumanResources.Positions;
using BUS.ImportExport;
using BUS.Settings;
using DTO.HumanResources.Departments;
using DTO.HumanResources.Employees;
using DTO.HumanResources.Positions;
using GUI.Forms.HumanResources.Contracts;
using GUI.Forms.Lookups;

namespace GUI.Forms.HumanResources.Employees;

/// <summary>
/// Màn hình Hồ sơ Nhân viên tích hợp đầy đủ:
/// - Thuộc tính ERD ([H2])
/// - Tách chế độ Thêm mới và Sửa với Mã NV chỉ đọc khi sửa ([H3])
/// - Mở form Hợp đồng Main–Sub ([H4])
/// - Nút tra cứu Phòng ban và Chức vụ ([H5])
/// - Tìm kiếm nâng cao AND/OR/NOT trên ArrayList ở BUS ([H6])
/// - Nhập và xuất Excel .xls ([H7])
/// - Đọc tham số lương cơ bản tối thiểu từ DB ([H8])
/// </summary>
public sealed class EmployeeForm : Form
{
    private readonly string _connection_string;
    private readonly EmployeeService _employee_service;
    private readonly DepartmentService _department_service;
    private readonly PositionService _position_service;
    private readonly EmployeeExcelService _excel_service;
    private readonly SystemParameterService _parameter_service;

    private ArrayList _cached_items = new();
    private ArrayList _displayed_items = new();
    private readonly List<EmployeeSearchCondition> _advanced_conditions = [];
    private int _selected_id;
    private int? _selected_department_id;
    private string _selected_department_code = string.Empty;
    private int? _selected_position_id;
    private string _selected_position_code = string.Empty;
    private bool _is_edit_mode;
    private decimal _min_base_salary = 4960000m;

    private readonly Label _mode_label = new()
    {
        Location = new Point(16, 10),
        AutoSize = true,
        Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
        ForeColor = Color.FromArgb(24, 90, 140),
        Text = "Chế độ: Thêm mới nhân viên"
    };
    private readonly Label _param_hint_label = new()
    {
        Location = new Point(360, 11),
        AutoSize = true,
        ForeColor = Color.DimGray,
        Text = "Lương CB tối thiểu theo tham số: đang tải..."
    };

    private readonly TextBox _txt_code = new() { Location = new Point(110, 38), Width = 145, MaxLength = 20 };
    private readonly TextBox _txt_last_name = new() { Location = new Point(110, 70), Width = 145, MaxLength = 100 };
    private readonly TextBox _txt_first_name = new() { Location = new Point(110, 102), Width = 145, MaxLength = 50 };
    private readonly DateTimePicker _dtp_dob = new() { Location = new Point(110, 134), Width = 145, Format = DateTimePickerFormat.Short };
    private readonly ComboBox _cmb_gender = new() { Location = new Point(110, 166), Width = 145, DropDownStyle = ComboBoxStyle.DropDownList };

    private readonly TextBox _txt_phone = new() { Location = new Point(365, 38), Width = 175, MaxLength = 20 };
    private readonly TextBox _txt_email = new() { Location = new Point(365, 70), Width = 175, MaxLength = 254 };
    private readonly TextBox _txt_dept_display = new() { Location = new Point(365, 102), Width = 115, ReadOnly = true, PlaceholderText = "Chọn phòng ban..." };
    private readonly Button _btn_dept_lookup = new() { Text = "…", Location = new Point(484, 101), Width = 30, Height = 25 };
    private readonly Button _btn_dept_clear = new() { Text = "X", Location = new Point(516, 101), Width = 26, Height = 25 };
    private readonly TextBox _txt_pos_display = new() { Location = new Point(365, 134), Width = 115, ReadOnly = true, PlaceholderText = "Chọn chức vụ..." };
    private readonly Button _btn_pos_lookup = new() { Text = "…", Location = new Point(484, 133), Width = 30, Height = 25 };
    private readonly Button _btn_pos_clear = new() { Text = "X", Location = new Point(516, 133), Width = 26, Height = 25 };
    private readonly DateTimePicker _dtp_hire = new() { Location = new Point(365, 166), Width = 175, Format = DateTimePickerFormat.Short };

    private readonly TextBox _txt_salary = new() { Location = new Point(665, 38), Width = 165, PlaceholderText = "0" };
    private readonly ComboBox _cmb_status = new() { Location = new Point(665, 70), Width = 165, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _cmb_degree = new() { Location = new Point(665, 102), Width = 165, DropDownStyle = ComboBoxStyle.DropDown };

    private readonly Button _btn_new = new() { Text = "Làm mới (Thêm)", Location = new Point(860, 36), Width = 125, Height = 28 };
    private readonly Button _btn_add = new() { Text = "Thêm nhân viên", Location = new Point(995, 36), Width = 125, Height = 28 };
    private readonly Button _btn_update = new() { Text = "Cập nhật (Sửa)", Location = new Point(860, 70), Width = 125, Height = 28 };
    private readonly Button _btn_delete = new() { Text = "Xóa nhân viên", Location = new Point(995, 70), Width = 125, Height = 28 };
    private readonly Button _btn_import_xls = new() { Text = "Nhập Excel (.xls)", Location = new Point(860, 104), Width = 125, Height = 28 };
    private readonly Button _btn_export_xls = new() { Text = "Xuất Excel (.xls)", Location = new Point(995, 104), Width = 125, Height = 28 };
    private readonly Button _btn_contracts = new() { Text = "Hợp đồng (Main–Sub)", Location = new Point(860, 138), Width = 155, Height = 28 };
    private readonly Button _btn_load = new() { Text = "Tải lại", Location = new Point(1025, 138), Width = 95, Height = 28 };

    // Khu vực tìm kiếm cơ bản & nâng cao
    private readonly TextBox _txt_quick_search = new() { Location = new Point(110, 206), Width = 240, PlaceholderText = "Tìm nhanh theo mã, tên, phòng ban, SĐT..." };
    private readonly ComboBox _cmb_logic = new() { Location = new Point(470, 206), Width = 70, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _cmb_adv_field = new() { Location = new Point(546, 206), Width = 155, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _cmb_adv_op = new() { Location = new Point(706, 206), Width = 68, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox _txt_adv_value = new() { Location = new Point(780, 206), Width = 145, PlaceholderText = "Giá trị (vd: 01/01/2024)" };
    private readonly Button _btn_add_cond = new() { Text = "+ ĐK", Location = new Point(930, 204), Width = 52, Height = 27 };
    private readonly Button _btn_apply_adv = new() { Text = "Lọc nâng cao", Location = new Point(986, 204), Width = 100, Height = 27 };
    private readonly Button _btn_clear_adv = new() { Text = "Bỏ lọc", Location = new Point(1090, 204), Width = 68, Height = 27 };
    private readonly ListBox _lst_conditions = new() { Location = new Point(470, 238), Size = new Size(575, 44) };
    private readonly Button _btn_remove_cond = new() { Text = "Xóa ĐK chọn", Location = new Point(1052, 245), Width = 106, Height = 28 };

    private readonly ListView _employee_list = new()
    {
        Location = new Point(16, 290),
        Size = new Size(1142, 410),
        Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
        View = View.Details,
        FullRowSelect = true,
        MultiSelect = false,
        HideSelection = false
    };

    public EmployeeForm(string? connection_string = null)
    {
        _connection_string = connection_string
                             ?? Environment.GetEnvironmentVariable("HRM_CONNECTION_STRING")
                             ?? "Server=localhost;Database=HumanResourceManagement;Integrated Security=True;TrustServerCertificate=True";
        _employee_service = HumanResourcesServiceFactory.CreateEmployeeService(_connection_string);
        _department_service = HumanResourcesServiceFactory.CreateDepartmentService(_connection_string);
        _position_service = HumanResourcesServiceFactory.CreatePositionService(_connection_string);
        _excel_service = HumanResourcesServiceFactory.CreateEmployeeExcelService(_connection_string);
        _parameter_service = ServiceFactory.CreateSystemParameterService(_connection_string);

        Text = "Quản lý Hồ sơ Nhân viên";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(1180, 720);
        MinimumSize = new Size(1080, 640);
        Font = new Font("Segoe UI", 9.5f);

        InitializeDropdowns();
        InitializeListColumns();
        AddFormControls();
        HookEvents();

        SwitchToAddMode();
        Shown += async (_, _) => await LoadDataAsync();
    }

    private void InitializeDropdowns()
    {
        _cmb_gender.Items.AddRange(["Nam", "Nữ", "Khác"]);
        _cmb_gender.SelectedIndex = 0;

        _cmb_status.Items.AddRange(["Đang làm việc", "Thử việc", "Tạm nghỉ", "Đã nghỉ việc"]);
        _cmb_status.SelectedIndex = 0;

        _cmb_degree.Items.AddRange(["", "Trung cấp", "Cao đẳng", "Đại học", "Thạc sĩ", "Tiến sĩ"]);

        _cmb_logic.Items.AddRange(["AND", "OR", "NOT"]);
        _cmb_logic.SelectedIndex = 0;

        var fields = new List<KeyValuePair<string, string>>
        {
            new(nameof(EmployeeDto.FullName), "Họ và tên"),
            new(nameof(EmployeeDto.EmployeeCode), "Mã nhân viên"),
            new(nameof(EmployeeDto.DepartmentCode), "Mã phòng ban"),
            new(nameof(EmployeeDto.DepartmentName), "Tên phòng ban"),
            new(nameof(EmployeeDto.PositionCode), "Mã chức vụ"),
            new(nameof(EmployeeDto.PositionName), "Tên chức vụ"),
            new(nameof(EmployeeDto.Gender), "Giới tính"),
            new(nameof(EmployeeDto.HireDate), "Ngày vào làm"),
            new(nameof(EmployeeDto.DateOfBirth), "Ngày sinh"),
            new(nameof(EmployeeDto.CurrentBaseSalary), "Lương CB hiện hành"),
            new(nameof(EmployeeDto.Status), "Trạng thái"),
            new(nameof(EmployeeDto.Degree), "Bằng cấp"),
            new(nameof(EmployeeDto.PhoneNumber), "Số điện thoại"),
            new(nameof(EmployeeDto.Email), "Email")
        };
        _cmb_adv_field.DisplayMember = "Value";
        _cmb_adv_field.ValueMember = "Key";
        _cmb_adv_field.DataSource = fields;

        _cmb_adv_op.Items.AddRange(["LIKE", "=", "<>", ">", ">=", "<", "<="]);
        _cmb_adv_op.SelectedIndex = 0;
    }

    private void InitializeListColumns()
    {
        _employee_list.Columns.Add("Mã NV", 85);
        _employee_list.Columns.Add("Họ", 110);
        _employee_list.Columns.Add("Tên", 80);
        _employee_list.Columns.Add("Ngày sinh", 95);
        _employee_list.Columns.Add("Giới tính", 70);
        _employee_list.Columns.Add("Điện thoại", 100);
        _employee_list.Columns.Add("Email", 155);
        _employee_list.Columns.Add("Phòng ban", 130);
        _employee_list.Columns.Add("Chức vụ", 110);
        _employee_list.Columns.Add("Ngày vào làm", 95);
        _employee_list.Columns.Add("Lương CB", 105);
        _employee_list.Columns.Add("Trạng thái", 110);
        _employee_list.Columns.Add("Bằng cấp", 95);
    }

    private void AddFormControls()
    {
        Controls.AddRange([
            _mode_label, _param_hint_label,
            new Label { Text = "Mã NV:", Location = new Point(16, 41), AutoSize = true }, _txt_code,
            new Label { Text = "Họ:", Location = new Point(16, 73), AutoSize = true }, _txt_last_name,
            new Label { Text = "Tên:", Location = new Point(16, 105), AutoSize = true }, _txt_first_name,
            new Label { Text = "Ngày sinh:", Location = new Point(16, 137), AutoSize = true }, _dtp_dob,
            new Label { Text = "Giới tính:", Location = new Point(16, 169), AutoSize = true }, _cmb_gender,

            new Label { Text = "Điện thoại:", Location = new Point(275, 41), AutoSize = true }, _txt_phone,
            new Label { Text = "Email:", Location = new Point(275, 73), AutoSize = true }, _txt_email,
            new Label { Text = "Phòng ban:", Location = new Point(275, 105), AutoSize = true }, _txt_dept_display, _btn_dept_lookup, _btn_dept_clear,
            new Label { Text = "Chức vụ:", Location = new Point(275, 137), AutoSize = true }, _txt_pos_display, _btn_pos_lookup, _btn_pos_clear,
            new Label { Text = "Ngày vào:", Location = new Point(275, 169), AutoSize = true }, _dtp_hire,

            new Label { Text = "Lương CB:", Location = new Point(560, 41), AutoSize = true }, _txt_salary,
            new Label { Text = "Trạng thái:", Location = new Point(560, 73), AutoSize = true }, _cmb_status,
            new Label { Text = "Bằng cấp:", Location = new Point(560, 105), AutoSize = true }, _cmb_degree,

            _btn_new, _btn_add, _btn_update, _btn_delete,
            _btn_import_xls, _btn_export_xls, _btn_contracts, _btn_load,

            new Label { Text = "Tìm nhanh:", Location = new Point(16, 209), AutoSize = true }, _txt_quick_search,
            new Label { Text = "Tìm nâng cao:", Location = new Point(365, 209), AutoSize = true },
            _cmb_logic, _cmb_adv_field, _cmb_adv_op, _txt_adv_value,
            _btn_add_cond, _btn_apply_adv, _btn_clear_adv,
            _lst_conditions, _btn_remove_cond,
            _employee_list
        ]);
    }

    private void HookEvents()
    {
        _btn_load.Click += async (_, _) => await LoadDataAsync();
        _btn_new.Click += (_, _) => SwitchToAddMode();
        _btn_add.Click += async (_, _) => await SaveEmployeeAsync(is_edit_mode: false);
        _btn_update.Click += async (_, _) => await SaveEmployeeAsync(is_edit_mode: true);
        _btn_delete.Click += async (_, _) => await DeleteSelectedAsync();

        _btn_dept_lookup.Click += async (_, _) => await PickDepartmentAsync();
        _btn_dept_clear.Click += (_, _) => ClearDepartment();
        _btn_pos_lookup.Click += async (_, _) => await PickPositionAsync();
        _btn_pos_clear.Click += (_, _) => ClearPosition();

        _txt_quick_search.TextChanged += (_, _) =>
        {
            ArrayList filtered = _employee_service.Search(_cached_items, _txt_quick_search.Text);
            RenderEmployees(filtered);
        };

        _btn_add_cond.Click += (_, _) => AddSearchCondition();
        _btn_remove_cond.Click += (_, _) => RemoveSelectedCondition();
        _btn_apply_adv.Click += (_, _) => ApplyAdvancedSearch();
        _btn_clear_adv.Click += (_, _) => ClearAdvancedSearch();

        _btn_export_xls.Click += (_, _) => ExportExcel();
        _btn_import_xls.Click += async (_, _) => await ImportExcelAsync();
        _btn_contracts.Click += (_, _) => OpenContractsMainSub();

        _employee_list.SelectedIndexChanged += (_, _) => SelectFromList();
    }

    private void SwitchToAddMode()
    {
        _is_edit_mode = false;
        _selected_id = 0;
        _txt_code.Clear();
        _txt_code.ReadOnly = false;
        _txt_last_name.Clear();
        _txt_first_name.Clear();
        _dtp_dob.Value = new DateTime(2000, 1, 1);
        _cmb_gender.SelectedIndex = 0;
        _txt_phone.Clear();
        _txt_email.Clear();
        ClearDepartment();
        ClearPosition();
        _dtp_hire.Value = DateTime.Today;
        _txt_salary.Text = _min_base_salary.ToString("0.##", CultureInfo.InvariantCulture);
        _cmb_status.SelectedIndex = 0;
        _cmb_degree.Text = string.Empty;
        _employee_list.SelectedItems.Clear();

        _btn_add.Enabled = true;
        _btn_update.Enabled = false;
        _btn_delete.Enabled = false;
        _mode_label.Text = "Chế độ: Thêm mới nhân viên";
    }

    private void SelectFromList()
    {
        if (_employee_list.SelectedItems.Count != 1
            || _employee_list.SelectedItems[0].Tag is not EmployeeDto emp)
            return;

        emp.SynchronizeNameFields();
        _is_edit_mode = true;
        _selected_id = emp.Id;
        _txt_code.Text = emp.EmployeeCode;
        // Khi sửa hồ sơ đang chọn, giữ mã nhân viên ở chế độ chỉ đọc.
        _txt_code.ReadOnly = true;
        _txt_last_name.Text = emp.LastName;
        _txt_first_name.Text = emp.FirstName;
        _dtp_dob.Value = emp.DateOfBirth < _dtp_dob.MinDate ? new DateTime(2000, 1, 1) : emp.DateOfBirth;
        _cmb_gender.SelectedItem = emp.Gender;
        _txt_phone.Text = emp.PhoneNumber;
        _txt_email.Text = emp.Email ?? string.Empty;

        _selected_department_id = emp.DepartmentId;
        _selected_department_code = emp.DepartmentCode;
        _txt_dept_display.Text = string.IsNullOrEmpty(emp.DepartmentCode)
            ? string.Empty
            : $"{emp.DepartmentCode} - {emp.DepartmentName}";

        _selected_position_id = emp.PositionId;
        _selected_position_code = emp.PositionCode;
        _txt_pos_display.Text = string.IsNullOrEmpty(emp.PositionCode)
            ? string.Empty
            : $"{emp.PositionCode} - {emp.PositionName}";

        _dtp_hire.Value = emp.HireDate < _dtp_hire.MinDate ? DateTime.Today : emp.HireDate;
        _txt_salary.Text = emp.CurrentBaseSalary.ToString("0.##", CultureInfo.InvariantCulture);
        _cmb_status.SelectedItem = emp.Status;
        _cmb_degree.Text = emp.Degree ?? string.Empty;

        _btn_add.Enabled = false;
        _btn_update.Enabled = true;
        _btn_delete.Enabled = true;
        _mode_label.Text = $"Chế độ: Đang sửa nhân viên [{emp.EmployeeCode}]";
    }

    private async Task LoadDataAsync()
    {
        try
        {
            _min_base_salary = await _parameter_service.GetDecimalValueAsync("MIN_BASE_SALARY", 4960000m);
            _param_hint_label.Text = $"Lương CB tối thiểu theo tham số hệ thống: {_min_base_salary:N0} VNĐ";
            if (!_is_edit_mode && string.IsNullOrWhiteSpace(_txt_code.Text))
                _txt_salary.Text = _min_base_salary.ToString("0.##", CultureInfo.InvariantCulture);

            List<EmployeeDto> list = await _employee_service.GetAllAsync();
            _cached_items = new ArrayList(list);
            ArrayList visible = _advanced_conditions.Count > 0
                ? _employee_service.SearchAdvanced(_cached_items, _advanced_conditions)
                : _employee_service.Search(_cached_items, _txt_quick_search.Text);
            RenderEmployees(visible);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Lỗi tải danh sách nhân viên", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task SaveEmployeeAsync(bool is_edit_mode)
    {
        try
        {
            string raw_salary = string.IsNullOrWhiteSpace(_txt_salary.Text) ? "0" : _txt_salary.Text.Trim();
            if (!decimal.TryParse(raw_salary, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal salary)
                && !decimal.TryParse(raw_salary, NumberStyles.Number, CultureInfo.CurrentCulture, out salary))
            {
                throw new ArgumentException("Lương cơ bản hiện hành phải là số hợp lệ.");
            }

            var dto = new EmployeeDto
            {
                Id = is_edit_mode ? _selected_id : 0,
                EmployeeCode = _txt_code.Text,
                LastName = _txt_last_name.Text,
                FirstName = _txt_first_name.Text,
                DateOfBirth = _dtp_dob.Value.Date,
                Gender = _cmb_gender.SelectedItem?.ToString() ?? "Nam",
                PhoneNumber = _txt_phone.Text,
                Email = _txt_email.Text,
                DepartmentId = _selected_department_id,
                DepartmentCode = _selected_department_code,
                PositionId = _selected_position_id,
                PositionCode = _selected_position_code,
                HireDate = _dtp_hire.Value.Date,
                CurrentBaseSalary = salary,
                Status = _cmb_status.SelectedItem?.ToString() ?? "Đang làm việc",
                Degree = _cmb_degree.Text
            };

            await _employee_service.SaveAsync(dto, is_edit_mode);
            SwitchToAddMode();
            await LoadDataAsync();
            MessageBox.Show(this, is_edit_mode ? "Đã cập nhật hồ sơ nhân viên." : "Đã thêm hồ sơ nhân viên mới.",
                "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            MessageBox.Show(this, ex.Message, "Kiểm tra hồ sơ nhân viên", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private async Task DeleteSelectedAsync()
    {
        if (!_is_edit_mode || string.IsNullOrWhiteSpace(_txt_code.Text))
            return;

        if (MessageBox.Show(this, $"Xóa nhân viên '{_txt_code.Text}'?", "Xác nhận xóa",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        try
        {
            await _employee_service.DeleteAsync(_txt_code.Text);
            SwitchToAddMode();
            await LoadDataAsync();
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            MessageBox.Show(this, ex.Message, "Không thể xóa", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private async Task PickDepartmentAsync()
    {
        try
        {
            List<DepartmentDto> departments = await _department_service.GetAllAsync();
            using var lookup = new DepartmentLookupForm(_department_service, departments);
            if (lookup.ShowDialog(this) != DialogResult.OK)
                return;

            DepartmentDto? chosen = DepartmentLookupForm.TakeSelection();
            if (chosen is null)
                return;

            _selected_department_id = chosen.Id;
            _selected_department_code = chosen.Code;
            _txt_dept_display.Text = $"{chosen.Code} - {chosen.Name}";
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Lỗi tra cứu phòng ban", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task PickPositionAsync()
    {
        try
        {
            List<PositionDto> positions = await _position_service.GetAllAsync();
            using var lookup = new PositionLookupForm(_position_service, positions);
            if (lookup.ShowDialog(this) != DialogResult.OK)
                return;

            PositionDto? chosen = PositionLookupForm.TakeSelection();
            if (chosen is null)
                return;

            _selected_position_id = chosen.Id;
            _selected_position_code = chosen.Code;
            _txt_pos_display.Text = $"{chosen.Code} - {chosen.Name}";
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Lỗi tra cứu chức vụ", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ClearDepartment()
    {
        _selected_department_id = null;
        _selected_department_code = string.Empty;
        _txt_dept_display.Clear();
    }

    private void ClearPosition()
    {
        _selected_position_id = null;
        _selected_position_code = string.Empty;
        _txt_pos_display.Clear();
    }

    private void AddSearchCondition()
    {
        string field_name = _cmb_adv_field.SelectedValue?.ToString() ?? nameof(EmployeeDto.FullName);
        string field_label = _cmb_adv_field.Text;
        string logic = _cmb_logic.SelectedItem?.ToString() ?? "AND";
        string op = _cmb_adv_op.SelectedItem?.ToString() ?? "LIKE";
        string val = _txt_adv_value.Text.Trim();

        if (string.IsNullOrWhiteSpace(val))
        {
            MessageBox.Show(this, "Vui lòng nhập giá trị cho điều kiện tìm kiếm.", "Tìm kiếm nâng cao",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var cond = new EmployeeSearchCondition
        {
            LogicalOperator = logic,
            FieldName = field_name,
            ComparisonOperator = op,
            Value = val
        };

        // Kiểm tra thử điều kiện qua BUS ngay khi thêm để báo lỗi kiểu dữ liệu sớm.
        try
        {
            _employee_service.SearchAdvanced(new ArrayList(), [cond]);
        }
        catch (ArgumentException ex)
        {
            MessageBox.Show(this, ex.Message, "Kiểm tra điều kiện lọc", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        _advanced_conditions.Add(cond);
        _lst_conditions.Items.Add($"[{logic}] {field_label} {op} \"{val}\"");
        _txt_adv_value.Clear();
    }

    private void RemoveSelectedCondition()
    {
        int index = _lst_conditions.SelectedIndex;
        if (index < 0 || index >= _advanced_conditions.Count)
            return;

        _advanced_conditions.RemoveAt(index);
        _lst_conditions.Items.RemoveAt(index);
    }

    private void ApplyAdvancedSearch()
    {
        try
        {
            if (_advanced_conditions.Count == 0 && !string.IsNullOrWhiteSpace(_txt_adv_value.Text))
                AddSearchCondition();

            if (_advanced_conditions.Count == 0)
            {
                MessageBox.Show(this, "Hãy thêm ít nhất một điều kiện để lọc nâng cao.", "Tìm kiếm nâng cao",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            ArrayList result = _employee_service.SearchAdvanced(_cached_items, _advanced_conditions);
            RenderEmployees(result);
        }
        catch (ArgumentException ex)
        {
            MessageBox.Show(this, ex.Message, "Điều kiện tìm kiếm không hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void ClearAdvancedSearch()
    {
        _advanced_conditions.Clear();
        _lst_conditions.Items.Clear();
        _txt_adv_value.Clear();
        _txt_quick_search.Clear();
        RenderEmployees(_cached_items);
    }

    private void ExportExcel()
    {
        try
        {
            using var dialog = new SaveFileDialog
            {
                Filter = "Excel 97-2003 (*.xls)|*.xls",
                FileName = $"DanhSachNhanVien_{DateTime.Now:yyyyMMdd_HHmm}.xls",
                Title = "Xuất danh sách nhân viên ra Excel .xls"
            };
            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            _excel_service.ExportToXls(dialog.FileName, _displayed_items);
            MessageBox.Show(this, $"Đã xuất {_displayed_items.Count} nhân viên ra tệp:\n{dialog.FileName}",
                "Xuất Excel thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Lỗi xuất Excel", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task ImportExcelAsync()
    {
        try
        {
            using var dialog = new OpenFileDialog
            {
                Filter = "Excel 97-2003 (*.xls)|*.xls|Tất cả tệp (*.*)|*.*",
                Title = "Chọn tệp Excel .xls danh sách nhân viên để nhập"
            };
            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            EmployeeImportResultDto result = await _excel_service.ImportFromXlsAsync(dialog.FileName);
            if (result.HasErrors)
            {
                string preview = string.Join(Environment.NewLine, result.Errors.Take(15));
                if (result.Errors.Count > 15)
                    preview += $"{Environment.NewLine}... và {result.Errors.Count - 15} lỗi khác.";

                MessageBox.Show(this,
                    $"Không thể nhập dữ liệu vì phát hiện {result.Errors.Count} lỗi:\n\n{preview}",
                    "Lỗi dữ liệu nhập Excel", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            await LoadDataAsync();
            MessageBox.Show(this, $"Đã nhập thành công {result.ImportedCount} nhân viên từ tệp Excel.",
                "Nhập Excel thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Lỗi nhập Excel", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void OpenContractsMainSub()
    {
        using var contract_form = new ContractForm(_connection_string, _txt_code.Text);
        contract_form.ShowDialog(this);
        _ = LoadDataAsync();
    }

    private void RenderEmployees(ArrayList source)
    {
        _displayed_items = new ArrayList(source);
        _employee_list.BeginUpdate();
        try
        {
            _employee_list.Items.Clear();
            foreach (object? obj in source)
            {
                if (obj is not EmployeeDto emp)
                    continue;

                emp.SynchronizeNameFields();
                _employee_list.Items.Add(new ListViewItem([
                    emp.EmployeeCode,
                    emp.LastName,
                    emp.FirstName,
                    emp.DateOfBirth.ToString("dd/MM/yyyy"),
                    emp.Gender,
                    emp.PhoneNumber,
                    emp.Email ?? string.Empty,
                    string.IsNullOrEmpty(emp.DepartmentCode) ? string.Empty : $"{emp.DepartmentCode} - {emp.DepartmentName}",
                    string.IsNullOrEmpty(emp.PositionCode) ? string.Empty : $"{emp.PositionCode} - {emp.PositionName}",
                    emp.HireDate.ToString("dd/MM/yyyy"),
                    emp.CurrentBaseSalary.ToString("N0"),
                    emp.Status,
                    emp.Degree ?? string.Empty
                ])
                {
                    Tag = emp
                });
            }
        }
        finally
        {
            _employee_list.EndUpdate();
        }
    }
}

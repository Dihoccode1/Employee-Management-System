using System.Collections;
using BUS.HumanResources.Departments;
using BUS.HumanResources.Employees;
using BUS.ProjectManagement;
using BUS.ProjectManagement.Projects;
using DTO.HumanResources.Departments;
using DTO.HumanResources.Employees;
using DTO.ProjectManagement.Projects;
using GUI.Forms.Lookups;

namespace GUI.Forms.ProjectManagement.Projects;

public sealed class ProjectForm : ManagementFormBase
{
    private readonly ProjectService _service;
    private readonly ProjectLookupService _lookups;
    private readonly DepartmentService? _department_service;
    private readonly EmployeeService? _employee_service;
    private ArrayList _items = new();
    private readonly List<ProjectSearchCondition> _advanced_conditions = [];
    private bool _use_advanced_filter;

    private readonly TextBox _nameBox = new() { MaxLength = 200 };
    private readonly DateTimePicker _startDateBox = DateBox(false);
    private readonly DateTimePicker _endDateBox = DateBox(true);
    private readonly DateTimePicker _deadlineBox = DateBox(false);
    private readonly TextBox _descriptionBox = new() { MaxLength = 2000 };
    private readonly NumericUpDown _budgetBox = NumberBox(9999999999999999.99m);
    private readonly ComboBox _departmentIdBox = ChoiceBox();
    private readonly ComboBox _managerIdBox = ChoiceBox();
    private readonly TextBox _statusBox = new() { MaxLength = 50 };

    // Các điều khiển tìm kiếm nâng cao AND/OR/NOT cho Dự án.
    private readonly ComboBox _adv_logic_box = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 72 };
    private readonly ComboBox _adv_field_box = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 140 };
    private readonly ComboBox _adv_op_box = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 76 };
    private readonly TextBox _adv_value_box = new() { Width = 150, PlaceholderText = "Giá trị / dd/MM/yyyy..." };
    private readonly Label _adv_summary_label = new() { AutoSize = true, Padding = new Padding(4, 6, 0, 0) };

    public ProjectForm(
        ProjectService service,
        ProjectLookupService lookups,
        DepartmentService? department_service = null,
        EmployeeService? employee_service = null)
        : base("Quản lý dự án")
    {
        _service = service;
        _lookups = lookups;
        _department_service = department_service;
        _employee_service = employee_service;

        AddField("Tên", _nameBox);
        AddField("Ngày bắt đầu", _startDateBox);
        AddField("Ngày kết thúc thực tế", _endDateBox);
        AddField("Hạn hoàn thành", _deadlineBox);
        AddField("Mô tả", _descriptionBox);
        AddField("Ngân sách", _budgetBox);
        if (_department_service is not null)
            AddFieldWithLookup("Phòng ban", _departmentIdBox, (_, _) => _ = LookupDepartmentAsync());
        else
            AddField("Phòng ban", _departmentIdBox);

        if (_employee_service is not null)
            AddFieldWithLookup("Người quản lý", _managerIdBox, (_, _) => _ = LookupManagerAsync());
        else
            AddField("Người quản lý", _managerIdBox);
        AddField("Trạng thái", _statusBox);

        AddColumn(nameof(ProjectDto.Id), "Mã", null);
        AddColumn(nameof(ProjectDto.Name), "Tên", null);
        AddColumn(nameof(ProjectDto.DepartmentName), "Phòng ban", null);
        AddColumn(nameof(ProjectDto.ManagerName), "Người quản lý", null);
        AddColumn(nameof(ProjectDto.Status), "Trạng thái", null);
        AddColumn(nameof(ProjectDto.Budget), "Ngân sách", "N2");
        AddColumn(nameof(ProjectDto.StartDate), "Ngày bắt đầu", "dd/MM/yyyy");
        AddColumn(nameof(ProjectDto.Deadline), "Hạn hoàn thành", "dd/MM/yyyy");
        AddColumn(nameof(ProjectDto.EndDate), "Ngày kết thúc thực tế", "dd/MM/yyyy");
        AddColumn(nameof(ProjectDto.Description), "Mô tả", null);

        BuildAdvancedSearchBar();
    }

    private void BuildAdvancedSearchBar()
    {
        _adv_logic_box.Items.AddRange(["AND", "OR", "NOT"]);
        _adv_logic_box.SelectedIndex = 0;

        _adv_field_box.DisplayMember = nameof(FieldChoice.Label);
        _adv_field_box.ValueMember = nameof(FieldChoice.Key);
        _adv_field_box.DataSource = new List<FieldChoice>
        {
            new("Name", "Tên dự án"),
            new("Status", "Trạng thái"),
            new("DepartmentName", "Phòng ban"),
            new("ManagerName", "Người quản lý"),
            new("StartDate", "Ngày bắt đầu"),
            new("Deadline", "Hạn hoàn thành"),
            new("EndDate", "Ngày kết thúc"),
            new("Budget", "Ngân sách")
        };

        _adv_op_box.Items.AddRange(["LIKE", "=", "<>", ">", ">=", "<", "<="]);
        _adv_op_box.SelectedIndex = 0;

        var bar = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(8, 4, 8, 4),
            WrapContents = true,
            AutoScroll = true
        };
        bar.Controls.Add(new Label { Text = "Lọc nâng cao:", AutoSize = true, Padding = new Padding(0, 6, 4, 0) });
        bar.Controls.Add(_adv_logic_box);
        bar.Controls.Add(_adv_field_box);
        bar.Controls.Add(_adv_op_box);
        bar.Controls.Add(_adv_value_box);

        var add_cond_btn = new Button { Text = "+ Thêm ĐK", AutoSize = true };
        add_cond_btn.Click += (_, _) => AddAdvancedCondition();

        var apply_btn = new Button { Text = "Áp dụng lọc", AutoSize = true };
        apply_btn.Click += (_, _) => ApplyAdvancedFilter();

        var clear_btn = new Button { Text = "Xóa ĐK", AutoSize = true };
        clear_btn.Click += (_, _) => ClearAdvancedConditions();

        bar.Controls.Add(add_cond_btn);
        bar.Controls.Add(apply_btn);
        bar.Controls.Add(clear_btn);
        bar.Controls.Add(_adv_summary_label);

        ExtraToolbarPanel.Controls.Add(bar);
        SetExtraToolbarHeight(46);
    }

    private void AddAdvancedCondition()
    {
        if (_adv_field_box.SelectedItem is not FieldChoice field)
            return;

        var cond = new ProjectSearchCondition
        {
            LogicalOperator = _adv_logic_box.SelectedItem?.ToString() ?? "AND",
            FieldName = field.Key,
            ComparisonOperator = _adv_op_box.SelectedItem?.ToString() ?? "LIKE",
            Value = _adv_value_box.Text.Trim()
        };
        try
        {
            // Kiểm tra định dạng ngay khi thêm điều kiện để báo lỗi sớm.
            _ = _service.SearchAdvanced(_items, [cond]);
            _advanced_conditions.Add(cond);
            _adv_value_box.Clear();
            UpdateAdvancedSummary();
            ApplyAdvancedFilter();
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            MessageBox.Show(this, ex.Message, "Điều kiện tìm kiếm", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void ApplyAdvancedFilter()
    {
        try
        {
            if (_advanced_conditions.Count == 0 && !string.IsNullOrWhiteSpace(_adv_value_box.Text))
            {
                if (_adv_field_box.SelectedItem is FieldChoice field)
                {
                    _advanced_conditions.Add(new ProjectSearchCondition
                    {
                        LogicalOperator = _adv_logic_box.SelectedItem?.ToString() ?? "AND",
                        FieldName = field.Key,
                        ComparisonOperator = _adv_op_box.SelectedItem?.ToString() ?? "LIKE",
                        Value = _adv_value_box.Text.Trim()
                    });
                    _adv_value_box.Clear();
                    UpdateAdvancedSummary();
                }
            }

            _use_advanced_filter = _advanced_conditions.Count > 0;
            ApplySearch();
            SetStatusMessage($"Đang hiển thị {Grid.Rows.Count} dự án (lọc nâng cao).");
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            MessageBox.Show(this, ex.Message, "Điều kiện tìm kiếm", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void ClearAdvancedConditions()
    {
        _advanced_conditions.Clear();
        _adv_value_box.Clear();
        _use_advanced_filter = false;
        UpdateAdvancedSummary();
        ApplySearch();
        SetStatusMessage($"Đang hiển thị {Grid.Rows.Count} dự án.");
    }

    private void UpdateAdvancedSummary()
    {
        if (_advanced_conditions.Count == 0)
        {
            _adv_summary_label.Text = "";
            return;
        }

        _adv_summary_label.Text = "ĐK: " + string.Join(" | ",
            _advanced_conditions.Select((c, idx) =>
                idx == 0
                    ? (c.LogicalOperator == "NOT" ? $"NOT {c.FieldName} {c.ComparisonOperator} '{c.Value}'" : $"{c.FieldName} {c.ComparisonOperator} '{c.Value}'")
                    : $"{c.LogicalOperator} {c.FieldName} {c.ComparisonOperator} '{c.Value}'"));
    }

    private async Task LookupDepartmentAsync()
    {
        if (_department_service is null)
            return;

        try
        {
            List<DepartmentDto> depts = await _department_service.GetAllAsync();
            using var dialog = new DepartmentLookupForm(_department_service, depts);
            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            DepartmentDto? selected = DepartmentLookupForm.TakeSelection();
            if (selected is not null)
                _departmentIdBox.SelectedValue = selected.Id;
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Lỗi tra cứu phòng ban", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task LookupManagerAsync()
    {
        if (_employee_service is null)
            return;

        try
        {
            List<EmployeeDto> employees = await _employee_service.GetAllAsync();
            using var dialog = new EmployeeLookupForm(_employee_service, employees);
            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            EmployeeDto? selected = EmployeeLookupForm.TakeSelection();
            if (selected is not null)
                _managerIdBox.SelectedValue = selected.Id;
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Lỗi tra cứu người quản lý", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    protected override async Task LoadRecordsAsync()
    {
        BindChoices(_departmentIdBox, await _lookups.GetDepartmentsAsync());
        BindChoices(_managerIdBox, await _lookups.GetEmployeesAsync());
        _items = new ArrayList(await _service.GetAllAsync());
    }

    protected override void ApplySearch()
    {
        List<ProjectDto> source_list = _use_advanced_filter
            ? _service.SearchAdvanced(_items, _advanced_conditions)
            : _service.Search(_items, SearchBox.Text);

        if (_use_advanced_filter && !string.IsNullOrWhiteSpace(SearchBox.Text))
            source_list = _service.Search(new ArrayList(source_list), SearchBox.Text);

        Grid.DataSource = source_list;
    }

    protected override void FillInputs()
    {
        if (Grid.CurrentRow?.DataBoundItem is not ProjectDto item) return;
        SelectedId = item.Id;
        _nameBox.Text = item.Name;
        SetDate(_startDateBox, item.StartDate);
        SetDate(_endDateBox, item.EndDate);
        SetDate(_deadlineBox, item.Deadline);
        _descriptionBox.Text = item.Description;
        _budgetBox.Value = item.Budget;
        _departmentIdBox.SelectedValue = item.DepartmentId;
        _managerIdBox.SelectedValue = item.ManagerId;
        _statusBox.Text = item.Status;
    }

    protected override void ClearInputs()
    {
        _nameBox.Text = "";
        SetDate(_startDateBox, DateTime.Today);
        SetDate(_endDateBox, null);
        SetDate(_deadlineBox, DateTime.Today);
        _descriptionBox.Text = "";
        _budgetBox.Value = 0;
        _departmentIdBox.SelectedIndex = -1;
        _managerIdBox.SelectedIndex = -1;
        _statusBox.Text = "Chưa bắt đầu";
    }

    protected override Task<int> SaveRecordAsync()
    {
        // GUI đọc ô nhập; BUS chịu trách nhiệm kiểm tra trước khi lưu.
        var item = new ProjectDto
        {
            Id = SelectedId,
            Name = _nameBox.Text,
            StartDate = _startDateBox.Value.Date,
            EndDate = OptionalDate(_endDateBox),
            Deadline = _deadlineBox.Value.Date,
            Description = _descriptionBox.Text,
            Budget = _budgetBox.Value,
            DepartmentId = ChoiceId(_departmentIdBox),
            ManagerId = ChoiceId(_managerIdBox),
            Status = _statusBox.Text,
        };
        return _service.SaveAsync(item);
    }

    protected override Task DeleteRecordAsync()
    {
        return _service.DeleteAsync(SelectedId);
    }

    private sealed record FieldChoice(string Key, string Label);
}

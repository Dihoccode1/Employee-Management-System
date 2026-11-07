using System.Collections;
using BUS.ProjectManagement;
using DTO.ProjectManagement;
using BUS.ProjectManagement.Projects;
using DTO.ProjectManagement.Projects;

namespace GUI.Forms.ProjectManagement.Projects;

public sealed class ProjectForm : ManagementFormBase
{
    private readonly ProjectService _service;
    private readonly ProjectLookupService _lookups;
    private ArrayList _items = new();

    private readonly TextBox _nameBox = new() { MaxLength = 200 };
    private readonly DateTimePicker _startDateBox = DateBox(false);
    private readonly DateTimePicker _endDateBox = DateBox(true);
    private readonly DateTimePicker _deadlineBox = DateBox(false);
    private readonly TextBox _descriptionBox = new() { MaxLength = 2000 };
    private readonly NumericUpDown _budgetBox = NumberBox(9999999999999999.99m);
    private readonly ComboBox _departmentIdBox = ChoiceBox();
    private readonly ComboBox _managerIdBox = ChoiceBox();
    private readonly TextBox _statusBox = new() { MaxLength = 50 };

    public ProjectForm(ProjectService service, ProjectLookupService lookups)
        : base("Quản lý dự án")
    {
        _service = service;
        _lookups = lookups;

        AddField("Tên", _nameBox);
        AddField("Ngày bắt đầu", _startDateBox);
        AddField("Ngày kết thúc thực tế", _endDateBox);
        AddField("Hạn hoàn thành", _deadlineBox);
        AddField("Mô tả", _descriptionBox);
        AddField("Ngân sách", _budgetBox);
        AddField("Phòng ban", _departmentIdBox);
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
    }

    protected override async Task LoadRecordsAsync()
    {
        BindChoices(_departmentIdBox, await _lookups.GetDepartmentsAsync());
        BindChoices(_managerIdBox, await _lookups.GetEmployeesAsync());
        _items = new ArrayList(await _service.GetAllAsync());
    }

    protected override void ApplySearch()
    {
        Grid.DataSource = _service.Search(_items, SearchBox.Text);
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
}

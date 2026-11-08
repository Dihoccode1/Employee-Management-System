using System.Collections;
using BUS.ProjectManagement;
using DTO.ProjectManagement;
using BUS.ProjectManagement.Assignments;
using DTO.ProjectManagement.Assignments;
using BUS.ProjectManagement.Tasks;

namespace GUI.Forms.ProjectManagement.Assignments;

public sealed class AssignmentForm : ManagementFormBase
{
    private readonly AssignmentService _service;
    private readonly ProjectTaskService _tasks;
    private readonly ProjectLookupService _lookups;
    private ArrayList _items = new();

    private readonly ComboBox _taskIdBox = ChoiceBox();
    private readonly ComboBox _employeeIdBox = ChoiceBox();
    private readonly DateTimePicker _assignedDateBox = DateBox(false);
    private readonly DateTimePicker _startDateBox = DateBox(false);
    private readonly DateTimePicker _deadlineBox = DateBox(false);
    private readonly DateTimePicker _completedDateBox = DateBox(true);
    private readonly NumericUpDown _completionPercentageBox = NumberBox(100);
    private readonly TextBox _statusBox = new() { MaxLength = 50 };
    private readonly TextBox _notesBox = new() { MaxLength = 2000 };

    public AssignmentForm(AssignmentService service, ProjectTaskService tasks, ProjectLookupService lookups)
        : base("Quản lý phân công")
    {
        _service = service;
        _tasks = tasks;
        _lookups = lookups;

        AddField("Công việc", _taskIdBox);
        AddField("Nhân viên", _employeeIdBox);
        AddField("Ngày phân công", _assignedDateBox);
        AddField("Ngày bắt đầu", _startDateBox);
        AddField("Hạn hoàn thành", _deadlineBox);
        AddField("Ngày hoàn thành", _completedDateBox);
        AddField("Tiến độ (%)", _completionPercentageBox);
        AddField("Trạng thái", _statusBox);
        AddField("Ghi chú", _notesBox);

        AddColumn(nameof(AssignmentDto.Id), "Mã", null);
        AddColumn(nameof(AssignmentDto.TaskName), "Công việc", null);
        AddColumn(nameof(AssignmentDto.EmployeeName), "Nhân viên", null);
        AddColumn(nameof(AssignmentDto.ProjectName), "Dự án", null);
        AddColumn(nameof(AssignmentDto.CompletionPercentage), "Tiến độ (%)", "N2");
        AddColumn(nameof(AssignmentDto.Status), "Trạng thái", null);
        AddColumn(nameof(AssignmentDto.AssignedDate), "Ngày phân công", "dd/MM/yyyy");
        AddColumn(nameof(AssignmentDto.StartDate), "Ngày bắt đầu", "dd/MM/yyyy");
        AddColumn(nameof(AssignmentDto.Deadline), "Hạn hoàn thành", "dd/MM/yyyy");
        AddColumn(nameof(AssignmentDto.CompletedDate), "Ngày hoàn thành", "dd/MM/yyyy");
        AddColumn(nameof(AssignmentDto.Notes), "Ghi chú", null);
    }

    protected override async Task LoadRecordsAsync()
    {
        var tasks = await _tasks.GetAllAsync();
        BindChoices(_taskIdBox, tasks.Select(item => new LookupDto
        {
            Id = item.Id, Name = $"#{item.Id} - {item.Name} ({item.ProjectName})"
        }).ToList());
        BindChoices(_employeeIdBox, await _lookups.GetEmployeesAsync());
        _items = new ArrayList(await _service.GetAllAsync());
    }

    protected override void ApplySearch()
    {
        Grid.DataSource = _service.Search(_items, SearchBox.Text);
    }

    protected override void FillInputs()
    {
        if (Grid.CurrentRow?.DataBoundItem is not AssignmentDto item) return;
        SelectedId = item.Id;
        _taskIdBox.SelectedValue = item.TaskId;
        _employeeIdBox.SelectedValue = item.EmployeeId;
        SetDate(_assignedDateBox, item.AssignedDate);
        SetDate(_startDateBox, item.StartDate);
        SetDate(_deadlineBox, item.Deadline);
        SetDate(_completedDateBox, item.CompletedDate);
        _completionPercentageBox.Value = item.CompletionPercentage;
        _statusBox.Text = item.Status;
        _notesBox.Text = item.Notes;
    }

    protected override void ClearInputs()
    {
        _taskIdBox.SelectedIndex = -1;
        _employeeIdBox.SelectedIndex = -1;
        SetDate(_assignedDateBox, DateTime.Today);
        SetDate(_startDateBox, DateTime.Today);
        SetDate(_deadlineBox, DateTime.Today);
        SetDate(_completedDateBox, null);
        _completionPercentageBox.Value = 0;
        _statusBox.Text = "Chưa bắt đầu";
        _notesBox.Text = "";
    }

    protected override Task<int> SaveRecordAsync()
    {
        // GUI đọc ô nhập; BUS chịu trách nhiệm kiểm tra trước khi lưu.
        var item = new AssignmentDto
        {
            Id = SelectedId,
            TaskId = ChoiceId(_taskIdBox),
            EmployeeId = ChoiceId(_employeeIdBox),
            AssignedDate = _assignedDateBox.Value.Date,
            StartDate = _startDateBox.Value.Date,
            Deadline = _deadlineBox.Value.Date,
            CompletedDate = OptionalDate(_completedDateBox),
            CompletionPercentage = _completionPercentageBox.Value,
            Status = _statusBox.Text,
            Notes = _notesBox.Text,
        };
        return _service.SaveAsync(item);
    }

    protected override Task DeleteRecordAsync()
    {
        return _service.DeleteAsync(SelectedId);
    }
}

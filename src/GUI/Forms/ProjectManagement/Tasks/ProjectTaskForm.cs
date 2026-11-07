using System.Collections;
using BUS.ProjectManagement;
using DTO.ProjectManagement;
using BUS.ProjectManagement.Tasks;
using DTO.ProjectManagement.Tasks;
using BUS.ProjectManagement.ProjectDetails;

namespace GUI.Forms.ProjectManagement.Tasks;

public sealed class ProjectTaskForm : ManagementFormBase
{
    private readonly ProjectTaskService _service;
    private readonly ProjectDetailService _details;
    private ArrayList _items = new();

    private readonly ComboBox _projectDetailIdBox = ChoiceBox();
    private readonly TextBox _nameBox = new() { MaxLength = 200 };
    private readonly TextBox _descriptionBox = new() { MaxLength = 2000 };
    private readonly TextBox _typeBox = new() { MaxLength = 100 };
    private readonly TextBox _priorityBox = new() { MaxLength = 50 };
    private readonly DateTimePicker _startDateBox = DateBox(false);
    private readonly DateTimePicker _deadlineBox = DateBox(false);
    private readonly DateTimePicker _completedDateBox = DateBox(true);
    private readonly NumericUpDown _estimatedHoursBox = NumberBox(99999999.99m);
    private readonly NumericUpDown _completionPercentageBox = NumberBox(100);
    private readonly TextBox _statusBox = new() { MaxLength = 50 };
    private readonly TextBox _notesBox = new() { MaxLength = 2000 };

    public ProjectTaskForm(ProjectTaskService service, ProjectDetailService details)
        : base("Quản lý công việc")
    {
        _service = service;
        _details = details;

        AddField("Chi tiết dự án", _projectDetailIdBox);
        AddField("Tên", _nameBox);
        AddField("Mô tả", _descriptionBox);
        AddField("Loại công việc", _typeBox);
        AddField("Mức độ ưu tiên", _priorityBox);
        AddField("Ngày bắt đầu", _startDateBox);
        AddField("Hạn hoàn thành", _deadlineBox);
        AddField("Ngày hoàn thành", _completedDateBox);
        AddField("Số giờ dự kiến", _estimatedHoursBox);
        AddField("Tiến độ (%)", _completionPercentageBox);
        AddField("Trạng thái", _statusBox);
        AddField("Ghi chú", _notesBox);

        AddColumn(nameof(ProjectTaskDto.Id), "Mã", null);
        AddColumn(nameof(ProjectTaskDto.Name), "Tên", null);
        AddColumn(nameof(ProjectTaskDto.ProjectName), "Dự án", null);
        AddColumn(nameof(ProjectTaskDto.ProjectDetailId), "Chi tiết dự án", null);
        AddColumn(nameof(ProjectTaskDto.Status), "Trạng thái", null);
        AddColumn(nameof(ProjectTaskDto.CompletionPercentage), "Tiến độ (%)", "N2");
        AddColumn(nameof(ProjectTaskDto.Priority), "Mức độ ưu tiên", null);
        AddColumn(nameof(ProjectTaskDto.StartDate), "Ngày bắt đầu", "dd/MM/yyyy");
        AddColumn(nameof(ProjectTaskDto.Deadline), "Hạn hoàn thành", "dd/MM/yyyy");
        AddColumn(nameof(ProjectTaskDto.CompletedDate), "Ngày hoàn thành", "dd/MM/yyyy");
        AddColumn(nameof(ProjectTaskDto.EstimatedHours), "Số giờ dự kiến", "N2");
        AddColumn(nameof(ProjectTaskDto.Type), "Loại công việc", null);
        AddColumn(nameof(ProjectTaskDto.Description), "Mô tả", null);
        AddColumn(nameof(ProjectTaskDto.Notes), "Ghi chú", null);
    }

    protected override async Task LoadRecordsAsync()
    {
        var details = await _details.GetAllAsync();
        BindChoices(_projectDetailIdBox, details.Select(item => new LookupDto
        {
            Id = item.Id, Name = $"Chi tiết #{item.Id} - {item.ProjectName}"
        }).ToList());
        _items = new ArrayList(await _service.GetAllAsync());
    }

    protected override void ApplySearch()
    {
        Grid.DataSource = _service.Search(_items, SearchBox.Text);
    }

    protected override void FillInputs()
    {
        if (Grid.CurrentRow?.DataBoundItem is not ProjectTaskDto item) return;
        SelectedId = item.Id;
        _projectDetailIdBox.SelectedValue = item.ProjectDetailId;
        _nameBox.Text = item.Name;
        _descriptionBox.Text = item.Description;
        _typeBox.Text = item.Type;
        _priorityBox.Text = item.Priority;
        SetDate(_startDateBox, item.StartDate);
        SetDate(_deadlineBox, item.Deadline);
        SetDate(_completedDateBox, item.CompletedDate);
        _estimatedHoursBox.Value = item.EstimatedHours;
        _completionPercentageBox.Value = item.CompletionPercentage;
        _statusBox.Text = item.Status;
        _notesBox.Text = item.Notes;
    }

    protected override void ClearInputs()
    {
        _projectDetailIdBox.SelectedIndex = -1;
        _nameBox.Text = "";
        _descriptionBox.Text = "";
        _typeBox.Text = "";
        _priorityBox.Text = "";
        SetDate(_startDateBox, DateTime.Today);
        SetDate(_deadlineBox, DateTime.Today);
        SetDate(_completedDateBox, null);
        _estimatedHoursBox.Value = 0;
        _completionPercentageBox.Value = 0;
        _statusBox.Text = "Chưa bắt đầu";
        _notesBox.Text = "";
    }

    protected override Task<int> SaveRecordAsync()
    {
        // GUI đọc ô nhập; BUS chịu trách nhiệm kiểm tra trước khi lưu.
        var item = new ProjectTaskDto
        {
            Id = SelectedId,
            ProjectDetailId = ChoiceId(_projectDetailIdBox),
            Name = _nameBox.Text,
            Description = _descriptionBox.Text,
            Type = _typeBox.Text,
            Priority = _priorityBox.Text,
            StartDate = _startDateBox.Value.Date,
            Deadline = _deadlineBox.Value.Date,
            CompletedDate = OptionalDate(_completedDateBox),
            EstimatedHours = _estimatedHoursBox.Value,
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

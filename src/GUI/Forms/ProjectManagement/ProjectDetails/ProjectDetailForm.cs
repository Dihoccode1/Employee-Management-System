using System.Collections;
using BUS.ProjectManagement;
using DTO.ProjectManagement;
using BUS.ProjectManagement.ProjectDetails;
using DTO.ProjectManagement.ProjectDetails;
using BUS.ProjectManagement.Projects;

namespace GUI.Forms.ProjectManagement.ProjectDetails;

public sealed class ProjectDetailForm : ManagementFormBase
{
    private readonly ProjectDetailService _service;
    private readonly ProjectService _projects;
    private ArrayList _items = new();

    private readonly ComboBox _projectIdBox = ChoiceBox();
    private readonly NumericUpDown _completionPercentageBox = NumberBox(100);
    private readonly TextBox _statusBox = new() { MaxLength = 50 };
    private readonly NumericUpDown _allowanceBox = NumberBox(9999999999999999.99m);
    private readonly NumericUpDown _bonusBox = NumberBox(9999999999999999.99m);

    public ProjectDetailForm(ProjectDetailService service, ProjectService projects)
        : base("Quản lý chi tiết dự án")
    {
        _service = service;
        _projects = projects;

        AddField("Dự án", _projectIdBox);
        AddField("Tiến độ (%)", _completionPercentageBox);
        AddField("Trạng thái", _statusBox);
        AddField("Phụ cấp (nhập tay)", _allowanceBox);
        AddField("Thưởng (nhập tay)", _bonusBox);

        AddColumn(nameof(ProjectDetailDto.Id), "Mã", null);
        AddColumn(nameof(ProjectDetailDto.ProjectName), "Dự án", null);
        AddColumn(nameof(ProjectDetailDto.CompletionPercentage), "Tiến độ (%)", "N2");
        AddColumn(nameof(ProjectDetailDto.Status), "Trạng thái", null);
        AddColumn(nameof(ProjectDetailDto.Allowance), "Phụ cấp (nhập tay)", "N2");
        AddColumn(nameof(ProjectDetailDto.Bonus), "Thưởng (nhập tay)", "N2");
    }

    protected override async Task LoadRecordsAsync()
    {
        var projects = await _projects.GetAllAsync();
        BindChoices(_projectIdBox, projects.Select(item => new LookupDto
        {
            Id = item.Id, Name = $"#{item.Id} - {item.Name}"
        }).ToList());
        _items = new ArrayList(await _service.GetAllAsync());
    }

    protected override void ApplySearch()
    {
        Grid.DataSource = _service.Search(_items, SearchBox.Text);
    }

    protected override void FillInputs()
    {
        if (Grid.CurrentRow?.DataBoundItem is not ProjectDetailDto item) return;
        SelectedId = item.Id;
        _projectIdBox.SelectedValue = item.ProjectId;
        _completionPercentageBox.Value = item.CompletionPercentage;
        _statusBox.Text = item.Status;
        _allowanceBox.Value = item.Allowance;
        _bonusBox.Value = item.Bonus;
    }

    protected override void ClearInputs()
    {
        _projectIdBox.SelectedIndex = -1;
        _completionPercentageBox.Value = 0;
        _statusBox.Text = "Chưa bắt đầu";
        _allowanceBox.Value = 0;
        _bonusBox.Value = 0;
    }

    protected override Task<int> SaveRecordAsync()
    {
        // GUI đọc ô nhập; BUS chịu trách nhiệm kiểm tra trước khi lưu.
        var item = new ProjectDetailDto
        {
            Id = SelectedId,
            ProjectId = ChoiceId(_projectIdBox),
            CompletionPercentage = _completionPercentageBox.Value,
            Status = _statusBox.Text,
            Allowance = _allowanceBox.Value,
            Bonus = _bonusBox.Value,
        };
        return _service.SaveAsync(item);
    }

    protected override Task DeleteRecordAsync()
    {
        return _service.DeleteAsync(SelectedId);
    }
}

using System.Collections;
using BUS.ProjectManagement.Assignments;
using BUS.ProjectManagement.Projects;
using BUS.ProjectManagement.ProjectDetails;
using BUS.ProjectManagement.Tasks;
using BUS.ProjectManagement;
using DTO.ProjectManagement;
using DTO.ProjectManagement.Assignments;
using DTO.ProjectManagement.Projects;
using DTO.ProjectManagement.ProjectDetails;
using DTO.ProjectManagement.Tasks;

namespace GUI.Forms.ProjectManagement.Assignments;

public partial class AssignmentForm : Form
{
    private readonly AssignmentBus dataBus;
    private readonly ProjectBus projectBus;
    private readonly ProjectDetailBus detailBus;
    private readonly ProjectTaskBus taskBus;
    private readonly ProjectLookupBus lookupBus;
    private int selectedId;
    private bool isLoading;

    public AssignmentForm(AssignmentBus dataBus, ProjectBus projectBus, ProjectDetailBus detailBus, ProjectTaskBus taskBus, ProjectLookupBus lookupBus)
    {
        this.dataBus = dataBus;
        this.projectBus = projectBus;
        this.detailBus = detailBus;
        this.taskBus = taskBus;
        this.lookupBus = lookupBus;
        initializeControls();
        clearFields();
    }

    private void formShown(object? sender, EventArgs e)
    {
        reloadData();
    }

    private void loadLookups()
    {
        List<LookupItemDto> options = new List<LookupItemDto>();
        foreach (ProjectDto item in projectBus.getAll())
            options.Add(new LookupItemDto { id = item.id, name = item.id + " - " + item.name });
        ProjectFormControls.bindOptions(projectIdComboBox, options);
        ProjectFormControls.bindOptions(employeeIdComboBox, lookupBus.getEmployees());
        ProjectFormControls.bindOptions(taskIdComboBox, new List<LookupItemDto>());
    }

    private void reloadData()
    {
        isLoading = true;
        try
        {
            itemListView.Items.Clear();
            clearFields();
            loadLookups();
            showItems(dataBus.search(searchTextBox.Text));
        }
        catch (Exception exception)
        {
            ProjectFormControls.showError(exception);
        }
        finally
        {
            isLoading = false;
        }
    }

    private void showItems(ArrayList items)
    {
        itemListView.Items.Clear();
        foreach (AssignmentDto item in items)
        {
            ListViewItem row = new ListViewItem(item.id.ToString());
            row.SubItems.Add(item.taskId.ToString());
            row.SubItems.Add(item.projectId.ToString());
            row.SubItems.Add(item.employeeId.ToString());
            row.SubItems.Add(item.assignedDate.ToString("dd/MM/yyyy"));
            row.SubItems.Add(item.startDate.ToString("dd/MM/yyyy"));
            row.SubItems.Add(item.deadline.ToString("dd/MM/yyyy"));
            row.SubItems.Add(item.completedDate?.ToString("dd/MM/yyyy") ?? "");
            row.SubItems.Add(item.completionRate.ToString());
            row.SubItems.Add(item.contributionScore.ToString());
            row.SubItems.Add(item.status);
            row.SubItems.Add(item.notes);
            row.Tag = item;
            itemListView.Items.Add(row);
        }
    }

    private AssignmentDto readInput()
    {
        return new AssignmentDto
        {
            id = selectedId,
            taskId = ProjectFormControls.selectedId(taskIdComboBox),
            projectId = ProjectFormControls.selectedId(projectIdComboBox),
            employeeId = ProjectFormControls.selectedId(employeeIdComboBox),
            assignedDate = assignedDatePicker.Value.Date,
            startDate = startDatePicker.Value.Date,
            deadline = deadlinePicker.Value.Date,
            completedDate = completedDatePicker.Checked ? completedDatePicker.Value.Date : null,
            completionRate = completionRateInput.Value,
            contributionScore = contributionScoreInput.Value,
            status = statusComboBox.Text,
            notes = notesTextBox.Text
        };
    }

    private void clearFields()
    {
        selectedId = 0;
        idTextBox.Text = "(tự động)";
        taskIdComboBox.SelectedIndex = -1;
        projectIdComboBox.SelectedIndex = -1;
        employeeIdComboBox.SelectedIndex = -1;
        assignedDatePicker.Value = DateTime.Today;
        startDatePicker.Value = DateTime.Today;
        deadlinePicker.Value = DateTime.Today.AddMonths(1);
        completedDatePicker.Value = DateTime.Today;
        completedDatePicker.Checked = false;
        completionRateInput.Value = 0;
        contributionScoreInput.Value = 0;
        statusComboBox.SelectedItem = "Not started";
        notesTextBox.Clear();
        ProjectFormControls.bindOptions(taskIdComboBox, new List<LookupItemDto>());
    }

    private void itemSelectionChanged(object? sender, EventArgs e)
    {
        if (isLoading)
            return;
        if (itemListView.SelectedItems.Count == 0)
        {
            isLoading = true;
            try { clearFields(); }
            finally { isLoading = false; }
            return;
        }
        isLoading = true;
        try
        {
            AssignmentDto item = (AssignmentDto)itemListView.SelectedItems[0].Tag!;
            selectedId = item.id;
            idTextBox.Text = item.id.ToString();
            ProjectFormControls.selectId(projectIdComboBox, item.projectId);
            loadTaskOptions();
            ProjectFormControls.selectId(taskIdComboBox, item.taskId);
            ProjectFormControls.selectId(employeeIdComboBox, item.employeeId);
            assignedDatePicker.Value = item.assignedDate;
            startDatePicker.Value = item.startDate;
            deadlinePicker.Value = item.deadline;
            completedDatePicker.Value = item.completedDate ?? DateTime.Today;
            completedDatePicker.Checked = item.completedDate.HasValue;
            completionRateInput.Value = item.completionRate;
            contributionScoreInput.Value = item.contributionScore;
            statusComboBox.SelectedItem = item.status;
            notesTextBox.Text = item.notes;
        }
        catch (Exception exception)
        {
            clearFields();
            ProjectFormControls.showError(exception);
        }
        finally
        {
            isLoading = false;
        }
    }

    private void newButtonClick(object? sender, EventArgs e)
    {
        isLoading = true;
        try
        {
            itemListView.SelectedItems.Clear();
            clearFields();
        }
        finally { isLoading = false; }
    }

    private void addButtonClick(object? sender, EventArgs e)
    {
        if (selectedId > 0)
        {
            MessageBox.Show("Bấm Nhập mới trước khi thêm bản ghi.", "Thông báo");
            return;
        }
        try
        {
            dataBus.add(readInput());
            MessageBox.Show("Đã thêm thành công.", "Thông báo");
            reloadData();
        }
        catch (Exception exception) { ProjectFormControls.showError(exception); }
    }

    private void updateButtonClick(object? sender, EventArgs e)
    {
        if (selectedId <= 0)
        {
            MessageBox.Show("Hãy chọn bản ghi cần sửa.", "Thông báo");
            return;
        }
        try
        {
            dataBus.update(readInput());
            MessageBox.Show("Đã sửa thành công.", "Thông báo");
            reloadData();
        }
        catch (Exception exception) { ProjectFormControls.showError(exception); }
    }

    private void deleteButtonClick(object? sender, EventArgs e)
    {
        if (selectedId <= 0)
        {
            MessageBox.Show("Hãy chọn bản ghi cần xóa.", "Thông báo");
            return;
        }
        if (MessageBox.Show("Xóa bản ghi #" + selectedId + "?", "Xác nhận",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;
        try
        {
            dataBus.delete(selectedId);
            MessageBox.Show("Đã xóa thành công.", "Thông báo");
            reloadData();
        }
        catch (Exception exception) { ProjectFormControls.showError(exception); }
    }

    private void searchButtonClick(object? sender, EventArgs e)
    {
        reloadData();
    }

    private void reloadButtonClick(object? sender, EventArgs e)
    {
        searchTextBox.Clear();
        reloadData();
    }

    private void projectSelectionChanged(object? sender, EventArgs e)
    {
        if (isLoading)
            return;
        try
        {
            loadTaskOptions();
        }
        catch (Exception exception)
        {
            ProjectFormControls.bindOptions(taskIdComboBox, new List<LookupItemDto>());
            ProjectFormControls.showError(exception);
        }
    }

    private void loadTaskOptions()
    {
        List<LookupItemDto> options = new List<LookupItemDto>();
        int projectId = ProjectFormControls.selectedId(projectIdComboBox);
        if (projectId > 0)
        {
            foreach (ProjectDetailDto detail in detailBus.getAll())
            {
                if (detail.projectId != projectId)
                    continue;
                foreach (ProjectTaskDto task in taskBus.getByProjectDetail(detail.id))
                    options.Add(new LookupItemDto { id = task.id, name = task.id + " - " + task.name });
                break;
            }
        }
        ProjectFormControls.bindOptions(taskIdComboBox, options);
    }
}

using System.Collections;
using BUS.ProjectManagement.Tasks;
using BUS.ProjectManagement.ProjectDetails;
using DTO.ProjectManagement;
using DTO.ProjectManagement.Tasks;
using DTO.ProjectManagement.ProjectDetails;

namespace GUI.Forms.ProjectManagement.Tasks;

public partial class ProjectTaskForm : Form
{
    private readonly ProjectTaskBus dataBus;
    private readonly ProjectDetailBus detailBus;
    private int selectedId;
    private bool isLoading;

    public ProjectTaskForm(ProjectTaskBus dataBus, ProjectDetailBus detailBus)
    {
        this.dataBus = dataBus;
        this.detailBus = detailBus;
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
        foreach (ProjectDetailDto item in detailBus.getAll())
            options.Add(new LookupItemDto { id = item.id, name = "CT #" + item.id + " - Dự án #" + item.projectId });
        ProjectFormControls.bindOptions(projectDetailIdComboBox, options);
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
        foreach (ProjectTaskDto item in items)
        {
            ListViewItem row = new ListViewItem(item.id.ToString());
            row.SubItems.Add(item.projectDetailId.ToString());
            row.SubItems.Add(item.name);
            row.SubItems.Add(item.description);
            row.SubItems.Add(item.taskType);
            row.SubItems.Add(item.priority);
            row.SubItems.Add(item.startDate.ToString("dd/MM/yyyy"));
            row.SubItems.Add(item.deadline.ToString("dd/MM/yyyy"));
            row.SubItems.Add(item.completedDate?.ToString("dd/MM/yyyy") ?? "");
            row.SubItems.Add(item.estimatedHours.ToString());
            row.SubItems.Add(item.completionRate.ToString());
            row.SubItems.Add(item.status);
            row.SubItems.Add(item.notes);
            row.Tag = item;
            itemListView.Items.Add(row);
        }
    }

    private ProjectTaskDto readInput()
    {
        return new ProjectTaskDto
        {
            id = selectedId,
            projectDetailId = ProjectFormControls.selectedId(projectDetailIdComboBox),
            name = nameTextBox.Text,
            description = descriptionTextBox.Text,
            taskType = taskTypeTextBox.Text,
            priority = priorityComboBox.Text,
            startDate = startDatePicker.Value.Date,
            deadline = deadlinePicker.Value.Date,
            completedDate = completedDatePicker.Checked ? completedDatePicker.Value.Date : null,
            estimatedHours = estimatedHoursInput.Value,
            completionRate = completionRateInput.Value,
            status = statusComboBox.Text,
            notes = notesTextBox.Text
        };
    }

    private void clearFields()
    {
        selectedId = 0;
        idTextBox.Text = "(tự động)";
        projectDetailIdComboBox.SelectedIndex = -1;
        nameTextBox.Clear();
        descriptionTextBox.Clear();
        taskTypeTextBox.Clear();
        priorityComboBox.SelectedItem = "Normal";
        startDatePicker.Value = DateTime.Today;
        deadlinePicker.Value = DateTime.Today.AddMonths(1);
        completedDatePicker.Value = DateTime.Today;
        completedDatePicker.Checked = false;
        estimatedHoursInput.Value = 0;
        completionRateInput.Value = 0;
        statusComboBox.SelectedItem = "Not started";
        notesTextBox.Clear();
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
            ProjectTaskDto item = (ProjectTaskDto)itemListView.SelectedItems[0].Tag!;
            selectedId = item.id;
            idTextBox.Text = item.id.ToString();
            ProjectFormControls.selectId(projectDetailIdComboBox, item.projectDetailId);
            nameTextBox.Text = item.name;
            descriptionTextBox.Text = item.description;
            taskTypeTextBox.Text = item.taskType;
            priorityComboBox.SelectedItem = item.priority;
            startDatePicker.Value = item.startDate;
            deadlinePicker.Value = item.deadline;
            completedDatePicker.Value = item.completedDate ?? DateTime.Today;
            completedDatePicker.Checked = item.completedDate.HasValue;
            estimatedHoursInput.Value = item.estimatedHours;
            completionRateInput.Value = item.completionRate;
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
}

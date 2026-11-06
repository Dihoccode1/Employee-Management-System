using System.Collections;
using BUS.ProjectManagement.Projects;
using BUS.ProjectManagement.ProjectDetails;
using BUS.ProjectManagement.Tasks;
using BUS.ProjectManagement;
using DTO.ProjectManagement;
using DTO.ProjectManagement.Projects;
using DTO.ProjectManagement.ProjectDetails;
using DTO.ProjectManagement.Tasks;

namespace GUI.Forms.ProjectManagement.Projects;

public partial class ProjectForm : Form
{
    private readonly ProjectBus dataBus;
    private readonly ProjectDetailBus detailBus;
    private readonly ProjectTaskBus taskBus;
    private readonly ProjectLookupBus lookupBus;
    private int selectedId;
    private bool isLoading;

    public ProjectForm(ProjectBus dataBus, ProjectDetailBus detailBus, ProjectTaskBus taskBus, ProjectLookupBus lookupBus)
    {
        this.dataBus = dataBus;
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
        ProjectFormControls.bindOptions(departmentIdComboBox, lookupBus.getDepartments());
        ProjectFormControls.bindOptions(managerIdComboBox, lookupBus.getEmployees());
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
        foreach (ProjectDto item in items)
        {
            ListViewItem row = new ListViewItem(item.id.ToString());
            row.SubItems.Add(item.name);
            row.SubItems.Add(item.startDate.ToString("dd/MM/yyyy"));
            row.SubItems.Add(item.endDate?.ToString("dd/MM/yyyy") ?? "");
            row.SubItems.Add(item.deadline.ToString("dd/MM/yyyy"));
            row.SubItems.Add(item.description);
            row.SubItems.Add(item.budget.ToString());
            row.SubItems.Add(item.departmentId.ToString());
            row.SubItems.Add(item.managerId.ToString());
            row.SubItems.Add(item.status);
            row.Tag = item;
            itemListView.Items.Add(row);
        }
    }

    private ProjectDto readInput()
    {
        return new ProjectDto
        {
            id = selectedId,
            name = nameTextBox.Text,
            startDate = startDatePicker.Value.Date,
            endDate = endDatePicker.Checked ? endDatePicker.Value.Date : null,
            deadline = deadlinePicker.Value.Date,
            description = descriptionTextBox.Text,
            budget = budgetInput.Value,
            departmentId = ProjectFormControls.selectedId(departmentIdComboBox),
            managerId = ProjectFormControls.selectedId(managerIdComboBox),
            status = statusComboBox.Text
        };
    }

    private void clearFields()
    {
        selectedId = 0;
        idTextBox.Text = "(tự động)";
        nameTextBox.Clear();
        startDatePicker.Value = DateTime.Today;
        endDatePicker.Value = DateTime.Today;
        endDatePicker.Checked = false;
        deadlinePicker.Value = DateTime.Today.AddMonths(1);
        descriptionTextBox.Clear();
        budgetInput.Value = 0;
        departmentIdComboBox.SelectedIndex = -1;
        managerIdComboBox.SelectedIndex = -1;
        statusComboBox.SelectedItem = "Not started";
        taskListView.Items.Clear();
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
            ProjectDto item = (ProjectDto)itemListView.SelectedItems[0].Tag!;
            selectedId = item.id;
            idTextBox.Text = item.id.ToString();
            nameTextBox.Text = item.name;
            startDatePicker.Value = item.startDate;
            endDatePicker.Value = item.endDate ?? DateTime.Today;
            endDatePicker.Checked = item.endDate.HasValue;
            deadlinePicker.Value = item.deadline;
            descriptionTextBox.Text = item.description;
            budgetInput.Value = item.budget;
            ProjectFormControls.selectId(departmentIdComboBox, item.departmentId);
            ProjectFormControls.selectId(managerIdComboBox, item.managerId);
            statusComboBox.SelectedItem = item.status;
            showProjectTasks(item.id);
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

    private void showProjectTasks(int projectId)
    {
        taskListView.Items.Clear();
        int detailId = 0;
        foreach (ProjectDetailDto detail in detailBus.getAll())
        {
            if (detail.projectId == projectId)
            {
                detailId = detail.id;
                break;
            }
        }
        if (detailId == 0)
            return;
        foreach (ProjectTaskDto task in taskBus.getByProjectDetail(detailId))
        {
            ListViewItem row = new ListViewItem(task.id.ToString());
            row.SubItems.Add(task.name);
            row.SubItems.Add(task.completionRate.ToString());
            row.SubItems.Add(task.status);
            taskListView.Items.Add(row);
        }
    }
}

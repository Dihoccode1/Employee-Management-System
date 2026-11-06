using System.Collections;
using BUS.ProjectManagement.ProjectDetails;
using BUS.ProjectManagement.Projects;
using DTO.ProjectManagement;
using DTO.ProjectManagement.ProjectDetails;
using DTO.ProjectManagement.Projects;

namespace GUI.Forms.ProjectManagement.ProjectDetails;

public partial class ProjectDetailForm : Form
{
    private readonly ProjectDetailBus dataBus;
    private readonly ProjectBus projectBus;
    private int selectedId;
    private bool isLoading;

    public ProjectDetailForm(ProjectDetailBus dataBus, ProjectBus projectBus)
    {
        this.dataBus = dataBus;
        this.projectBus = projectBus;
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
        foreach (ProjectDetailDto item in items)
        {
            ListViewItem row = new ListViewItem(item.id.ToString());
            row.SubItems.Add(item.projectId.ToString());
            row.SubItems.Add(item.completionRate.ToString());
            row.SubItems.Add(item.status);
            row.SubItems.Add(item.allowance.ToString());
            row.SubItems.Add(item.reward.ToString());
            row.Tag = item;
            itemListView.Items.Add(row);
        }
    }

    private ProjectDetailDto readInput()
    {
        return new ProjectDetailDto
        {
            id = selectedId,
            projectId = ProjectFormControls.selectedId(projectIdComboBox),
            completionRate = completionRateInput.Value,
            status = statusComboBox.Text,
            allowance = allowanceInput.Value,
            reward = rewardInput.Value
        };
    }

    private void clearFields()
    {
        selectedId = 0;
        idTextBox.Text = "(tự động)";
        projectIdComboBox.SelectedIndex = -1;
        completionRateInput.Value = 0;
        statusComboBox.SelectedItem = "Not started";
        allowanceInput.Value = 0;
        rewardInput.Value = 0;
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
            ProjectDetailDto item = (ProjectDetailDto)itemListView.SelectedItems[0].Tag!;
            selectedId = item.id;
            idTextBox.Text = item.id.ToString();
            ProjectFormControls.selectId(projectIdComboBox, item.projectId);
            completionRateInput.Value = item.completionRate;
            statusComboBox.SelectedItem = item.status;
            allowanceInput.Value = item.allowance;
            rewardInput.Value = item.reward;
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

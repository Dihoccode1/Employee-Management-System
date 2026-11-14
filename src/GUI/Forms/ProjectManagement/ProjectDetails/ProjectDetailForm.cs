using System.Collections;
using BUS.ProjectManagement.ProjectDetails;
using BUS.ProjectManagement.Projects;
using DTO.ProjectManagement.ProjectDetails;
using DTO.ProjectManagement.Projects;

namespace GUI.Forms.ProjectManagement.ProjectDetails;

/// <summary>Form Main–Sub với 2 ListView: Dự án (chính) ở trên và Chi tiết dự án (con) ở dưới.</summary>
public sealed class ProjectDetailForm : Form
{
    private readonly ProjectDetailService _service;
    private readonly ProjectService _projects;
    private ArrayList _project_source = new();
    private ArrayList _detail_source = new();
    private int _selected_project_id;
    private int _selected_detail_id;

    private readonly TextBox _project_search_box = new() { Width = 220, PlaceholderText = "Tìm dự án theo tên/trạng thái..." };
    private readonly TextBox _detail_search_box = new() { Width = 220, PlaceholderText = "Tìm chi tiết theo trạng thái..." };
    private readonly ListView _project_list = new()
    {
        Dock = DockStyle.Fill,
        View = View.Details,
        FullRowSelect = true,
        MultiSelect = false,
        HideSelection = false,
        GridLines = true
    };
    private readonly ListView _detail_list = new()
    {
        Dock = DockStyle.Fill,
        View = View.Details,
        FullRowSelect = true,
        MultiSelect = false,
        HideSelection = false,
        GridLines = true
    };

    private readonly Label _project_label = new() { AutoSize = true, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), Text = "Chưa chọn dự án" };
    private readonly NumericUpDown _completion_box = new() { Minimum = 0, Maximum = 100, DecimalPlaces = 2, Width = 110 };
    private readonly TextBox _status_box = new() { MaxLength = 50, Width = 170 };
    private readonly NumericUpDown _allowance_box = new() { Minimum = 0, Maximum = 999999999999m, DecimalPlaces = 2, ThousandsSeparator = true, Width = 150 };
    private readonly NumericUpDown _bonus_box = new() { Minimum = 0, Maximum = 999999999999m, DecimalPlaces = 2, ThousandsSeparator = true, Width = 150 };
    private readonly Label _mode_label = new() { AutoSize = true, Text = "Đang thêm mới chi tiết" };
    private readonly Label _status_strip_label = new() { Dock = DockStyle.Fill, Padding = new Padding(8, 4, 0, 0) };

    public ProjectDetailForm(ProjectDetailService service, ProjectService projects)
    {
        _service = service;
        _projects = projects;

        Text = "Quản lý chi tiết dự án (Main–Sub)";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(1180, 760);
        MinimumSize = new Size(960, 640);
        Font = new Font("Segoe UI", 9.5f);

        ConfigureListViews();
        BuildLayout();
        ClearDetailInputs();
        Shown += async (_, _) => await ReloadAllAsync();
    }

    private void ConfigureListViews()
    {
        _project_list.Columns.Add("Mã DA", 70);
        _project_list.Columns.Add("Tên dự án", 250);
        _project_list.Columns.Add("Phòng ban", 170);
        _project_list.Columns.Add("Người quản lý", 170);
        _project_list.Columns.Add("Ngân sách", 130);
        _project_list.Columns.Add("Ngày bắt đầu", 110);
        _project_list.Columns.Add("Hạn hoàn thành", 110);
        _project_list.Columns.Add("Trạng thái", 130);
        _project_list.SelectedIndexChanged += (_, _) => OnProjectSelectionChanged();

        _detail_list.Columns.Add("Mã CT", 75);
        _detail_list.Columns.Add("Dự án", 260);
        _detail_list.Columns.Add("Tiến độ (%)", 110);
        _detail_list.Columns.Add("Phụ cấp (nhập tay)", 160);
        _detail_list.Columns.Add("Thưởng (nhập tay)", 160);
        _detail_list.Columns.Add("Trạng thái", 170);
        _detail_list.SelectedIndexChanged += (_, _) => OnDetailSelectionChanged();
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5 };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 46));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 118));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 54));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));

        // Thanh công cụ trên cùng
        var toolbar = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(8, 6, 8, 6), WrapContents = false };
        var reload_btn = new Button { Text = "Tải lại", AutoSize = true };
        reload_btn.Click += async (_, _) => await ReloadAllAsync();

        var add_btn = new Button { Text = "Thêm chi tiết", AutoSize = true };
        add_btn.Click += (_, _) => StartNewDetail();

        var save_btn = new Button { Text = "Lưu chi tiết", AutoSize = true };
        save_btn.Click += async (_, _) => await SaveDetailAsync();

        var delete_btn = new Button { Text = "Xóa chi tiết", AutoSize = true };
        delete_btn.Click += async (_, _) => await DeleteDetailAsync();

        _project_search_box.TextChanged += (_, _) => RenderProjects(_selected_project_id);
        _detail_search_box.TextChanged += (_, _) => RenderDetailsForSelectedProject(_selected_detail_id);

        toolbar.Controls.Add(new Label { Text = "Tìm DA:", AutoSize = true, Padding = new Padding(0, 6, 4, 0) });
        toolbar.Controls.Add(_project_search_box);
        toolbar.Controls.Add(new Label { Text = "Tìm CT:", AutoSize = true, Padding = new Padding(8, 6, 4, 0) });
        toolbar.Controls.Add(_detail_search_box);
        toolbar.Controls.Add(reload_btn);
        toolbar.Controls.Add(add_btn);
        toolbar.Controls.Add(save_btn);
        toolbar.Controls.Add(delete_btn);

        // Khung danh sách Dự án chính (Main)
        var main_group = new GroupBox { Text = "1. Danh sách Dự án (chọn 1 dự án để xem Chi tiết dự án)", Dock = DockStyle.Fill, Padding = new Padding(6) };
        main_group.Controls.Add(_project_list);

        // Khung nhập liệu Chi tiết dự án
        var editor_group = new GroupBox { Text = "Thông tin Chi tiết dự án", Dock = DockStyle.Fill, Padding = new Padding(8) };
        var editor_flow = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = true };
        editor_flow.Controls.Add(new Label { Text = "Dự án:", AutoSize = true, Padding = new Padding(0, 6, 4, 0) });
        editor_flow.Controls.Add(_project_label);
        editor_flow.Controls.Add(new Label { Text = "   Tiến độ (%):", AutoSize = true, Padding = new Padding(12, 6, 4, 0) });
        editor_flow.Controls.Add(_completion_box);
        editor_flow.Controls.Add(new Label { Text = "Trạng thái:", AutoSize = true, Padding = new Padding(12, 6, 4, 0) });
        editor_flow.Controls.Add(_status_box);
        editor_flow.Controls.Add(new Label { Text = "Phụ cấp:", AutoSize = true, Padding = new Padding(12, 6, 4, 0) });
        editor_flow.Controls.Add(_allowance_box);
        editor_flow.Controls.Add(new Label { Text = "Thưởng:", AutoSize = true, Padding = new Padding(12, 6, 4, 0) });
        editor_flow.Controls.Add(_bonus_box);
        editor_flow.Controls.Add(_mode_label);
        editor_group.Controls.Add(editor_flow);

        // Khung danh sách Chi tiết dự án con (Sub)
        var sub_group = new GroupBox { Text = "2. Chi tiết dự án thuộc dự án đang chọn", Dock = DockStyle.Fill, Padding = new Padding(6) };
        sub_group.Controls.Add(_detail_list);

        root.Controls.Add(toolbar, 0, 0);
        root.Controls.Add(main_group, 0, 1);
        root.Controls.Add(editor_group, 0, 2);
        root.Controls.Add(sub_group, 0, 3);
        root.Controls.Add(_status_strip_label, 0, 4);
        Controls.Add(root);
    }

    private async Task ReloadAllAsync()
    {
        UseWaitCursor = true;
        try
        {
            List<ProjectDto> projects = await _projects.GetAllAsync();
            List<ProjectDetailDto> details = await _service.GetAllAsync();

            _project_source = new ArrayList(projects);
            _detail_source = new ArrayList(details);

            int preserve_project_id = _selected_project_id;
            int preserve_detail_id = _selected_detail_id;

            RenderProjects(preserve_project_id);
            RenderDetailsForSelectedProject(preserve_detail_id);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Lỗi tải dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            UseWaitCursor = false;
        }
    }

    private void RenderProjects(int select_project_id = 0)
    {
        _project_list.BeginUpdate();
        try
        {
            _project_list.Items.Clear();
            List<ProjectDto> filtered = _projects.Search(_project_source, _project_search_box.Text);
            foreach (ProjectDto project in filtered)
            {
                var item = new ListViewItem([
                    project.Id.ToString(),
                    project.Name,
                    project.DepartmentName,
                    project.ManagerName,
                    project.Budget.ToString("N0"),
                    project.StartDate.ToString("dd/MM/yyyy"),
                    project.Deadline.ToString("dd/MM/yyyy"),
                    project.Status
                ])
                {
                    Tag = project
                };
                if (project.Id == select_project_id)
                    item.Selected = true;
                _project_list.Items.Add(item);
            }
        }
        finally
        {
            _project_list.EndUpdate();
        }
    }

    private void OnProjectSelectionChanged()
    {
        if (_project_list.SelectedItems.Count == 1 && _project_list.SelectedItems[0].Tag is ProjectDto project)
        {
            _selected_project_id = project.Id;
            _project_label.Text = $"#{project.Id} - {project.Name}";
        }
        else
        {
            _selected_project_id = 0;
            _project_label.Text = "Chưa chọn dự án";
        }

        StartNewDetail();
        RenderDetailsForSelectedProject();
    }

    private void RenderDetailsForSelectedProject(int select_detail_id = 0)
    {
        _detail_list.BeginUpdate();
        try
        {
            _detail_list.Items.Clear();
            // Chỉ lấy các Chi tiết thuộc dự án đang chọn thông qua BUS trên ArrayList.
            List<ProjectDetailDto> items = _service.SearchByProject(_detail_source, _selected_project_id, _detail_search_box.Text);
            foreach (ProjectDetailDto detail in items)
            {
                var lvi = new ListViewItem([
                    detail.Id.ToString(),
                    detail.ProjectName,
                    detail.CompletionPercentage.ToString("N2"),
                    detail.Allowance.ToString("N0"),
                    detail.Bonus.ToString("N0"),
                    detail.Status
                ])
                {
                    Tag = detail
                };
                if (detail.Id == select_detail_id)
                    lvi.Selected = true;
                _detail_list.Items.Add(lvi);
            }

            _status_strip_label.Text = _selected_project_id > 0
                ? $"Dự án #{_selected_project_id}: có {_detail_list.Items.Count} chi tiết."
                : "Chọn một dự án ở danh sách trên để xem và cập nhật Chi tiết dự án.";
        }
        finally
        {
            _detail_list.EndUpdate();
        }
    }

    private void OnDetailSelectionChanged()
    {
        if (_detail_list.SelectedItems.Count != 1 || _detail_list.SelectedItems[0].Tag is not ProjectDetailDto item)
            return;

        _selected_detail_id = item.Id;
        _completion_box.Value = Math.Clamp(item.CompletionPercentage, _completion_box.Minimum, _completion_box.Maximum);
        _status_box.Text = item.Status;
        _allowance_box.Value = Math.Clamp(item.Allowance, _allowance_box.Minimum, _allowance_box.Maximum);
        _bonus_box.Value = Math.Clamp(item.Bonus, _bonus_box.Minimum, _bonus_box.Maximum);
        _mode_label.Text = $"Đang sửa chi tiết #{item.Id}";
    }

    private void StartNewDetail()
    {
        _selected_detail_id = 0;
        _detail_list.SelectedItems.Clear();
        ClearDetailInputs();
        _mode_label.Text = "Đang thêm mới chi tiết";
    }

    private void ClearDetailInputs()
    {
        _completion_box.Value = 0;
        _status_box.Text = "Chưa bắt đầu";
        _allowance_box.Value = 0;
        _bonus_box.Value = 0;
    }

    private async Task SaveDetailAsync()
    {
        try
        {
            if (_selected_project_id <= 0)
                throw new ArgumentException("Vui lòng chọn một dự án ở danh sách chính trước khi thêm hoặc sửa chi tiết.");

            var dto = new ProjectDetailDto
            {
                Id = _selected_detail_id,
                ProjectId = _selected_project_id,
                CompletionPercentage = _completion_box.Value,
                Status = _status_box.Text,
                Allowance = _allowance_box.Value,
                Bonus = _bonus_box.Value
            };
            int saved_id = await _service.SaveAsync(dto);
            _selected_detail_id = saved_id;
            await ReloadAllAsync();
            _status_strip_label.Text = $"Đã lưu chi tiết #{saved_id} cho dự án #{_selected_project_id}.";
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            MessageBox.Show(this, ex.Message, "Kiểm tra dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Lỗi lưu chi tiết dự án", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task DeleteDetailAsync()
    {
        try
        {
            if (_selected_detail_id <= 0)
                throw new ArgumentException("Vui lòng chọn một chi tiết ở danh sách dưới để xóa.");

            if (MessageBox.Show(this, $"Xóa chi tiết #{_selected_detail_id}?", "Xác nhận xóa",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            await _service.DeleteAsync(_selected_detail_id);
            _selected_detail_id = 0;
            await ReloadAllAsync();
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            MessageBox.Show(this, ex.Message, "Không thể xóa", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Lỗi xóa chi tiết dự án", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}

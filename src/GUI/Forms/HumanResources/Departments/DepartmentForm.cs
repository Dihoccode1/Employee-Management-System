using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using BUS.HumanResources;
using BUS.HumanResources.Departments;
using DTO.HumanResources.Departments;

namespace GUI.Forms.HumanResources.Departments;

/// <summary>Quản lý Phòng ban đầy đủ thuộc tính ERD và tách rõ chế độ Thêm mới / Sửa.</summary>
public sealed class DepartmentForm : Form
{
    private readonly DepartmentService _service;
    private ArrayList _cached_items = new();
    private int _selected_id;
    private bool _is_edit_mode;

    private readonly Label _mode_label = new()
    {
        Location = new Point(20, 12),
        AutoSize = true,
        Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
        ForeColor = Color.FromArgb(24, 90, 140),
        Text = "Chế độ: Thêm mới phòng ban"
    };
    private readonly TextBox _txt_code = new() { Location = new Point(150, 42), Width = 220, MaxLength = 20 };
    private readonly TextBox _txt_name = new() { Location = new Point(150, 76), Width = 220, MaxLength = 100 };
    private readonly TextBox _txt_description = new() { Location = new Point(150, 110), Width = 440, MaxLength = 500 };
    private readonly TextBox _txt_search = new() { Location = new Point(150, 148), Width = 260, PlaceholderText = "Tìm theo mã, tên hoặc mô tả..." };
    private readonly Button _btn_new = new() { Text = "Làm mới", Location = new Point(400, 40), Width = 90 };
    private readonly Button _btn_add = new() { Text = "Thêm mới", Location = new Point(500, 40), Width = 90 };
    private readonly Button _btn_update = new() { Text = "Cập nhật", Location = new Point(600, 40), Width = 90 };
    private readonly Button _btn_delete = new() { Text = "Xóa", Location = new Point(698, 40), Width = 80 };
    private readonly Button _btn_load = new() { Text = "Tải lại", Location = new Point(698, 108), Width = 80 };
    private readonly Button _btn_search = new() { Text = "Tìm", Location = new Point(420, 146), Width = 80 };

    private readonly ListView _department_list = new()
    {
        Location = new Point(20, 190),
        Size = new Size(760, 310),
        Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
        View = View.Details,
        FullRowSelect = true,
        MultiSelect = false,
        HideSelection = false
    };

    public DepartmentForm(string? connection_string = null)
    {
        string conn = connection_string
                      ?? Environment.GetEnvironmentVariable("HRM_CONNECTION_STRING")
                      ?? "Server=localhost;Database=HumanResourceManagement;Integrated Security=True;TrustServerCertificate=True";
        _service = HumanResourcesServiceFactory.CreateDepartmentService(conn);

        Text = "Quản lý Phòng ban";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(800, 520);
        MinimumSize = new Size(720, 460);
        Font = new Font("Segoe UI", 9.5f);

        _department_list.Columns.Add("ID", 55);
        _department_list.Columns.Add("Mã phòng ban", 130);
        _department_list.Columns.Add("Tên phòng ban", 230);
        _department_list.Columns.Add("Mô tả", 325);

        Controls.AddRange([
            _mode_label,
            new Label { Text = "Mã phòng ban:", Location = new Point(20, 45), AutoSize = true }, _txt_code,
            new Label { Text = "Tên phòng ban:", Location = new Point(20, 79), AutoSize = true }, _txt_name,
            new Label { Text = "Mô tả:", Location = new Point(20, 113), AutoSize = true }, _txt_description,
            new Label { Text = "Tìm kiếm:", Location = new Point(20, 151), AutoSize = true }, _txt_search,
            _btn_new, _btn_add, _btn_update, _btn_delete, _btn_load, _btn_search,
            _department_list
        ]);

        _btn_load.Click += async (_, _) => await LoadDataAsync();
        _btn_new.Click += (_, _) => SwitchToAddMode();
        _btn_add.Click += async (_, _) => await SaveAsync(is_edit_mode: false);
        _btn_update.Click += async (_, _) => await SaveAsync(is_edit_mode: true);
        _btn_delete.Click += async (_, _) => await DeleteSelectedAsync();
        _btn_search.Click += (_, _) => RenderList(_service.Search(_cached_items, _txt_search.Text));
        _txt_search.TextChanged += (_, _) => RenderList(_service.Search(_cached_items, _txt_search.Text));
        _department_list.SelectedIndexChanged += (_, _) => SelectFromList();

        SwitchToAddMode();
        Shown += async (_, _) => await LoadDataAsync();
    }

    private void SwitchToAddMode()
    {
        _is_edit_mode = false;
        _selected_id = 0;
        _txt_code.Clear();
        _txt_name.Clear();
        _txt_description.Clear();
        _txt_code.ReadOnly = false;
        _department_list.SelectedItems.Clear();
        _btn_add.Enabled = true;
        _btn_update.Enabled = false;
        _btn_delete.Enabled = false;
        _mode_label.Text = "Chế độ: Thêm mới phòng ban";
    }

    private void SelectFromList()
    {
        if (_department_list.SelectedItems.Count != 1 || _department_list.SelectedItems[0].Tag is not DepartmentDto dept)
            return;

        _is_edit_mode = true;
        _selected_id = dept.Id;
        _txt_code.Text = dept.Code;
        _txt_name.Text = dept.Name;
        _txt_description.Text = dept.Description;
        // Giữ trường mã khóa ở chế độ chỉ đọc khi đang sửa bản ghi đã chọn.
        _txt_code.ReadOnly = true;
        _btn_add.Enabled = false;
        _btn_update.Enabled = true;
        _btn_delete.Enabled = true;
        _mode_label.Text = $"Chế độ: Đang sửa phòng ban [{dept.Code}]";
    }

    private async Task LoadDataAsync()
    {
        try
        {
            List<DepartmentDto> list = await _service.GetAllAsync();
            _cached_items = new ArrayList(list);
            RenderList(_service.Search(_cached_items, _txt_search.Text));
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Lỗi tải dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task SaveAsync(bool is_edit_mode)
    {
        try
        {
            var dto = new DepartmentDto
            {
                Id = is_edit_mode ? _selected_id : 0,
                Code = _txt_code.Text,
                Name = _txt_name.Text,
                Description = _txt_description.Text
            };

            await _service.SaveAsync(dto, is_edit_mode);
            SwitchToAddMode();
            await LoadDataAsync();
            MessageBox.Show(this, is_edit_mode ? "Đã cập nhật phòng ban." : "Đã thêm phòng ban mới.",
                "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            MessageBox.Show(this, ex.Message, "Kiểm tra dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private async Task DeleteSelectedAsync()
    {
        if (!_is_edit_mode || string.IsNullOrWhiteSpace(_txt_code.Text))
        {
            MessageBox.Show(this, "Vui lòng chọn phòng ban cần xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (MessageBox.Show(this, $"Xóa phòng ban '{_txt_code.Text}'?", "Xác nhận xóa",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        try
        {
            await _service.DeleteAsync(_txt_code.Text);
            SwitchToAddMode();
            await LoadDataAsync();
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            MessageBox.Show(this, ex.Message, "Không thể xóa", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void RenderList(ArrayList source)
    {
        _department_list.BeginUpdate();
        try
        {
            _department_list.Items.Clear();
            foreach (object? obj in source)
            {
                if (obj is not DepartmentDto dept)
                    continue;

                _department_list.Items.Add(new ListViewItem([
                    dept.Id.ToString(),
                    dept.Code,
                    dept.Name,
                    dept.Description
                ])
                {
                    Tag = dept
                });
            }
        }
        finally
        {
            _department_list.EndUpdate();
        }
    }
}

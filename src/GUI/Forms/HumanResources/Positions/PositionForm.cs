using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Forms;
using BUS.HumanResources;
using BUS.HumanResources.Positions;
using DTO.HumanResources.Positions;

namespace GUI.Forms.HumanResources.Positions;

/// <summary>Quản lý Chức vụ đầy đủ thuộc tính ERD (mô tả, phụ cấp chức vụ) và tách chế độ Thêm / Sửa.</summary>
public sealed class PositionForm : Form
{
    private readonly PositionService _service;
    private ArrayList _cached_items = new();
    private int _selected_id;
    private bool _is_edit_mode;

    private readonly Label _mode_label = new()
    {
        Location = new Point(20, 12),
        AutoSize = true,
        Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
        ForeColor = Color.FromArgb(24, 90, 140),
        Text = "Chế độ: Thêm mới chức vụ"
    };
    private readonly TextBox _txt_code = new() { Location = new Point(150, 42), Width = 180, MaxLength = 20 };
    private readonly TextBox _txt_name = new() { Location = new Point(150, 76), Width = 180, MaxLength = 100 };
    private readonly TextBox _txt_allowance = new() { Location = new Point(470, 42), Width = 160, PlaceholderText = "0" };
    private readonly TextBox _txt_description = new() { Location = new Point(150, 110), Width = 480, MaxLength = 500 };
    private readonly TextBox _txt_search = new() { Location = new Point(150, 148), Width = 280, PlaceholderText = "Tìm theo mã, tên hoặc mô tả..." };

    private readonly Button _btn_new = new() { Text = "Làm mới", Location = new Point(650, 40), Width = 90 };
    private readonly Button _btn_add = new() { Text = "Thêm mới", Location = new Point(748, 40), Width = 90 };
    private readonly Button _btn_update = new() { Text = "Cập nhật", Location = new Point(650, 74), Width = 90 };
    private readonly Button _btn_delete = new() { Text = "Xóa", Location = new Point(748, 74), Width = 90 };
    private readonly Button _btn_load = new() { Text = "Tải lại", Location = new Point(748, 108), Width = 90 };
    private readonly Button _btn_search = new() { Text = "Tìm", Location = new Point(440, 146), Width = 80 };

    private readonly ListView _position_list = new()
    {
        Location = new Point(20, 190),
        Size = new Size(818, 310),
        Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
        View = View.Details,
        FullRowSelect = true,
        MultiSelect = false,
        HideSelection = false
    };

    public PositionForm(string? connection_string = null)
    {
        string conn = connection_string
                      ?? Environment.GetEnvironmentVariable("HRM_CONNECTION_STRING")
                      ?? "Server=localhost;Database=HumanResourceManagement;Integrated Security=True;TrustServerCertificate=True";
        _service = HumanResourcesServiceFactory.CreatePositionService(conn);

        Text = "Quản lý Chức vụ";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(860, 520);
        MinimumSize = new Size(760, 460);
        Font = new Font("Segoe UI", 9.5f);

        _position_list.Columns.Add("ID", 55);
        _position_list.Columns.Add("Mã chức vụ", 120);
        _position_list.Columns.Add("Tên chức vụ", 200);
        _position_list.Columns.Add("Phụ cấp chức vụ", 140);
        _position_list.Columns.Add("Mô tả", 280);

        Controls.AddRange([
            _mode_label,
            new Label { Text = "Mã chức vụ:", Location = new Point(20, 45), AutoSize = true }, _txt_code,
            new Label { Text = "Tên chức vụ:", Location = new Point(20, 79), AutoSize = true }, _txt_name,
            new Label { Text = "Phụ cấp CV:", Location = new Point(365, 45), AutoSize = true }, _txt_allowance,
            new Label { Text = "Mô tả:", Location = new Point(20, 113), AutoSize = true }, _txt_description,
            new Label { Text = "Tìm kiếm:", Location = new Point(20, 151), AutoSize = true }, _txt_search,
            _btn_new, _btn_add, _btn_update, _btn_delete, _btn_load, _btn_search,
            _position_list
        ]);

        _btn_load.Click += async (_, _) => await LoadDataAsync();
        _btn_new.Click += (_, _) => SwitchToAddMode();
        _btn_add.Click += async (_, _) => await SaveAsync(is_edit_mode: false);
        _btn_update.Click += async (_, _) => await SaveAsync(is_edit_mode: true);
        _btn_delete.Click += async (_, _) => await DeleteSelectedAsync();
        _btn_search.Click += (_, _) => RenderList(_service.Search(_cached_items, _txt_search.Text));
        _txt_search.TextChanged += (_, _) => RenderList(_service.Search(_cached_items, _txt_search.Text));
        _position_list.SelectedIndexChanged += (_, _) => SelectFromList();

        SwitchToAddMode();
        Shown += async (_, _) => await LoadDataAsync();
    }

    private void SwitchToAddMode()
    {
        _is_edit_mode = false;
        _selected_id = 0;
        _txt_code.Clear();
        _txt_name.Clear();
        _txt_allowance.Text = "0";
        _txt_description.Clear();
        _txt_code.ReadOnly = false;
        _position_list.SelectedItems.Clear();
        _btn_add.Enabled = true;
        _btn_update.Enabled = false;
        _btn_delete.Enabled = false;
        _mode_label.Text = "Chế độ: Thêm mới chức vụ";
    }

    private void SelectFromList()
    {
        if (_position_list.SelectedItems.Count != 1 || _position_list.SelectedItems[0].Tag is not PositionDto pos)
            return;

        _is_edit_mode = true;
        _selected_id = pos.Id;
        _txt_code.Text = pos.Code;
        _txt_name.Text = pos.Name;
        _txt_allowance.Text = pos.AllowanceAmount.ToString("0.##", CultureInfo.InvariantCulture);
        _txt_description.Text = pos.Description;
        _txt_code.ReadOnly = true;
        _btn_add.Enabled = false;
        _btn_update.Enabled = true;
        _btn_delete.Enabled = true;
        _mode_label.Text = $"Chế độ: Đang sửa chức vụ [{pos.Code}]";
    }

    private async Task LoadDataAsync()
    {
        try
        {
            List<PositionDto> list = await _service.GetAllAsync();
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
            string raw_allowance = string.IsNullOrWhiteSpace(_txt_allowance.Text) ? "0" : _txt_allowance.Text.Trim();
            if (!decimal.TryParse(raw_allowance, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal allowance)
                && !decimal.TryParse(raw_allowance, NumberStyles.Number, CultureInfo.CurrentCulture, out allowance))
            {
                throw new ArgumentException("Phụ cấp chức vụ phải là số hợp lệ.");
            }

            var dto = new PositionDto
            {
                Id = is_edit_mode ? _selected_id : 0,
                Code = _txt_code.Text,
                Name = _txt_name.Text,
                AllowanceAmount = allowance,
                Description = _txt_description.Text
            };

            await _service.SaveAsync(dto, is_edit_mode);
            SwitchToAddMode();
            await LoadDataAsync();
            MessageBox.Show(this, is_edit_mode ? "Đã cập nhật chức vụ." : "Đã thêm chức vụ mới.",
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
            MessageBox.Show(this, "Vui lòng chọn chức vụ cần xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (MessageBox.Show(this, $"Xóa chức vụ '{_txt_code.Text}'?", "Xác nhận xóa",
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
        _position_list.BeginUpdate();
        try
        {
            _position_list.Items.Clear();
            foreach (object? obj in source)
            {
                if (obj is not PositionDto pos)
                    continue;

                _position_list.Items.Add(new ListViewItem([
                    pos.Id.ToString(),
                    pos.Code,
                    pos.Name,
                    pos.AllowanceAmount.ToString("N0"),
                    pos.Description
                ])
                {
                    Tag = pos
                });
            }
        }
        finally
        {
            _position_list.EndUpdate();
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Forms;
using BUS;
using BUS.Settings;
using DTO.Settings;

namespace GUI.Forms.Settings;

/// <summary>Màn hình xem và cấu hình các tham số nghiệp vụ dùng chung (ngày công chuẩn, lương tối thiểu, ...).</summary>
public sealed class SystemParameterForm : Form
{
    private readonly SystemParameterService _service;
    private ArrayList _cached_items = new();
    private bool _is_edit_mode;

    private readonly Label _mode_label = new()
    {
        Location = new Point(20, 12),
        AutoSize = true,
        Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
        ForeColor = Color.FromArgb(24, 90, 140),
        Text = "Chế độ: Thêm mới tham số nghiệp vụ"
    };
    private readonly TextBox _txt_code = new() { Location = new Point(140, 42), Width = 210, MaxLength = 50 };
    private readonly TextBox _txt_name = new() { Location = new Point(140, 76), Width = 210, MaxLength = 150 };
    private readonly TextBox _txt_value = new() { Location = new Point(140, 110), Width = 210, MaxLength = 200 };
    private readonly ComboBox _cmb_type = new() { Location = new Point(470, 42), Width = 140, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox _txt_min = new() { Location = new Point(470, 76), Width = 140, PlaceholderText = "Không giới hạn" };
    private readonly TextBox _txt_max = new() { Location = new Point(720, 76), Width = 140, PlaceholderText = "Không giới hạn" };
    private readonly TextBox _txt_description = new() { Location = new Point(470, 110), Width = 390, MaxLength = 500 };
    private readonly TextBox _txt_search = new() { Location = new Point(140, 150), Width = 320, PlaceholderText = "Tìm theo mã, tên, giá trị hoặc mô tả..." };

    private readonly Button _btn_new = new() { Text = "Làm mới", Location = new Point(640, 40), Width = 80 };
    private readonly Button _btn_add = new() { Text = "Thêm mới", Location = new Point(728, 40), Width = 90 };
    private readonly Button _btn_update = new() { Text = "Cập nhật", Location = new Point(826, 40), Width = 90 };
    private readonly Button _btn_delete = new() { Text = "Xóa", Location = new Point(874, 74), Width = 80 };
    private readonly Button _btn_load = new() { Text = "Tải lại", Location = new Point(874, 108), Width = 80 };

    private readonly ListView _parameter_list = new()
    {
        Location = new Point(20, 192),
        Size = new Size(940, 330),
        Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
        View = View.Details,
        FullRowSelect = true,
        MultiSelect = false,
        HideSelection = false
    };

    public SystemParameterForm(SystemParameterService service)
    {
        _service = service;
        InitializeUi();
    }

    public SystemParameterForm(string? connection_string = null)
    {
        string conn = connection_string
                      ?? Environment.GetEnvironmentVariable("HRM_CONNECTION_STRING")
                      ?? "Server=localhost;Database=HumanResourceManagement;Integrated Security=True;TrustServerCertificate=True";
        _service = ServiceFactory.CreateSystemParameterService(conn);
        InitializeUi();
    }

    private void InitializeUi()
    {
        Text = "Tham số nghiệp vụ hệ thống";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(980, 540);
        MinimumSize = new Size(900, 480);
        Font = new Font("Segoe UI", 9.5f);

        _cmb_type.Items.AddRange(["INT", "DECIMAL", "STRING"]);
        _cmb_type.SelectedIndex = 1;

        _parameter_list.Columns.Add("Mã tham số", 190);
        _parameter_list.Columns.Add("Tên tham số", 210);
        _parameter_list.Columns.Add("Giá trị", 100);
        _parameter_list.Columns.Add("Kiểu", 75);
        _parameter_list.Columns.Add("Tối thiểu", 90);
        _parameter_list.Columns.Add("Tối đa", 95);
        _parameter_list.Columns.Add("Mô tả", 260);

        Controls.AddRange([
            _mode_label,
            new Label { Text = "Mã tham số:", Location = new Point(20, 45), AutoSize = true }, _txt_code,
            new Label { Text = "Tên tham số:", Location = new Point(20, 79), AutoSize = true }, _txt_name,
            new Label { Text = "Giá trị:", Location = new Point(20, 113), AutoSize = true }, _txt_value,
            new Label { Text = "Kiểu dữ liệu:", Location = new Point(370, 45), AutoSize = true }, _cmb_type,
            new Label { Text = "Giá trị Min:", Location = new Point(370, 79), AutoSize = true }, _txt_min,
            new Label { Text = "Giá trị Max:", Location = new Point(630, 79), AutoSize = true }, _txt_max,
            new Label { Text = "Mô tả:", Location = new Point(370, 113), AutoSize = true }, _txt_description,
            new Label { Text = "Tìm kiếm:", Location = new Point(20, 153), AutoSize = true }, _txt_search,
            _btn_new, _btn_add, _btn_update, _btn_delete, _btn_load,
            _parameter_list
        ]);

        _btn_load.Click += async (_, _) => await LoadDataAsync();
        _btn_new.Click += (_, _) => SwitchToAddMode();
        _btn_add.Click += async (_, _) => await SaveAsync(is_edit_mode: false);
        _btn_update.Click += async (_, _) => await SaveAsync(is_edit_mode: true);
        _btn_delete.Click += async (_, _) => await DeleteSelectedAsync();
        _txt_search.TextChanged += (_, _) => RenderList(_service.Search(_cached_items, _txt_search.Text));
        _parameter_list.SelectedIndexChanged += (_, _) => SelectFromList();

        SwitchToAddMode();
        Shown += async (_, _) => await LoadDataAsync();
    }

    private void SwitchToAddMode()
    {
        _is_edit_mode = false;
        _txt_code.Clear();
        _txt_name.Clear();
        _txt_value.Clear();
        _txt_min.Clear();
        _txt_max.Clear();
        _txt_description.Clear();
        _cmb_type.SelectedIndex = 1;
        _txt_code.ReadOnly = false;
        _parameter_list.SelectedItems.Clear();
        _btn_add.Enabled = true;
        _btn_update.Enabled = false;
        _btn_delete.Enabled = false;
        _mode_label.Text = "Chế độ: Thêm mới tham số nghiệp vụ";
    }

    private void SelectFromList()
    {
        if (_parameter_list.SelectedItems.Count != 1 || _parameter_list.SelectedItems[0].Tag is not SystemParameterDto param)
            return;

        _is_edit_mode = true;
        _txt_code.Text = param.Code;
        _txt_name.Text = param.Name;
        _txt_value.Text = param.Value;
        _cmb_type.SelectedItem = param.ValueType;
        _txt_min.Text = param.MinValue?.ToString("0.##", CultureInfo.InvariantCulture) ?? string.Empty;
        _txt_max.Text = param.MaxValue?.ToString("0.##", CultureInfo.InvariantCulture) ?? string.Empty;
        _txt_description.Text = param.Description;
        _txt_code.ReadOnly = true;
        _btn_add.Enabled = false;
        _btn_update.Enabled = true;
        _btn_delete.Enabled = true;
        _mode_label.Text = $"Chế độ: Đang sửa tham số [{param.Code}]";
    }

    private async Task LoadDataAsync()
    {
        try
        {
            List<SystemParameterDto> list = await _service.GetAllAsync();
            _cached_items = new ArrayList(list);
            RenderList(_service.Search(_cached_items, _txt_search.Text));
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Lỗi tải tham số", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task SaveAsync(bool is_edit_mode)
    {
        try
        {
            decimal? min_val = ParseOptionalDecimal(_txt_min.Text, "Giá trị tối thiểu");
            decimal? max_val = ParseOptionalDecimal(_txt_max.Text, "Giá trị tối đa");

            var dto = new SystemParameterDto
            {
                Code = _txt_code.Text,
                Name = _txt_name.Text,
                Value = _txt_value.Text,
                ValueType = _cmb_type.SelectedItem?.ToString() ?? "DECIMAL",
                MinValue = min_val,
                MaxValue = max_val,
                Description = _txt_description.Text
            };

            await _service.SaveAsync(dto, is_edit_mode);
            SwitchToAddMode();
            await LoadDataAsync();
            MessageBox.Show(this, is_edit_mode ? "Đã cập nhật tham số." : "Đã thêm tham số mới.",
                "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            MessageBox.Show(this, ex.Message, "Kiểm tra tham số", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private async Task DeleteSelectedAsync()
    {
        if (!_is_edit_mode || string.IsNullOrWhiteSpace(_txt_code.Text))
            return;

        if (MessageBox.Show(this, $"Xóa tham số '{_txt_code.Text}'?", "Xác nhận xóa",
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
        _parameter_list.BeginUpdate();
        try
        {
            _parameter_list.Items.Clear();
            foreach (object? obj in source)
            {
                if (obj is not SystemParameterDto param)
                    continue;

                _parameter_list.Items.Add(new ListViewItem([
                    param.Code,
                    param.Name,
                    param.Value,
                    param.ValueType,
                    param.MinValue?.ToString("N0") ?? "-",
                    param.MaxValue?.ToString("N0") ?? "-",
                    param.Description
                ])
                {
                    Tag = param
                });
            }
        }
        finally
        {
            _parameter_list.EndUpdate();
        }
    }

    private static decimal? ParseOptionalDecimal(string raw_text, string field_name)
    {
        if (string.IsNullOrWhiteSpace(raw_text))
            return null;

        string cleaned = raw_text.Trim();
        if (decimal.TryParse(cleaned, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal value)
            || decimal.TryParse(cleaned, NumberStyles.Number, CultureInfo.CurrentCulture, out value))
            return value;

        throw new ArgumentException($"{field_name} phải là số hợp lệ.");
    }
}

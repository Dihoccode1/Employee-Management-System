using System.Collections;
using BUS.HumanResources.Employees;
using BUS.SalaryInformation.Attendance;
using BUS.Settings;
using DTO.HumanResources.Employees;
using DTO.SalaryInformation.Attendance;
using DTO.SalaryInformation.AttendanceStatuses;
using GUI.Forms.Lookups;

namespace GUI.Forms.SalaryInformation.Attendance;

public sealed class AttendanceForm : Form
{
    private readonly AttendanceService _attendance_service;
    private readonly EmployeeService _employee_service;
    private readonly SystemParameterService? _parameter_service;
    private readonly TableLayoutPanel _layout = new() { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1 };
    private readonly ListView _status_list = CreateListView();
    private readonly ListView _attendance_list = CreateListView();
    private readonly TextBox _status_search = new() { Width = 200, PlaceholderText = "Tìm trạng thái" };
    private readonly TextBox _record_search = new() { Width = 240, PlaceholderText = "Tìm nhân viên hoặc ngày" };
    private readonly Label _parameter_label = new() { AutoSize = true, ForeColor = Color.DimGray, Margin = new Padding(8, 7, 0, 0) };
    private readonly TextBox _status_name = new() { Width = 220, MaxLength = 50 };
    private readonly TextBox _status_description = new() { Width = 440, MaxLength = 500 };
    private readonly TextBox _employee_name = new() { Width = 205, ReadOnly = true };
    private readonly DateTimePicker _work_date = new() { Width = 115, Format = DateTimePickerFormat.Short };
    private readonly DateTimePicker _check_in = CreateTimePicker();
    private readonly DateTimePicker _check_out = CreateTimePicker();
    private ArrayList _statuses = [];
    private ArrayList _records = [];
    private int _editing_status_id;
    private int _editing_record_id;
    private int _employee_id;
    private bool _rendering;
    private bool _busy;

    public AttendanceForm(AttendanceService attendance_service, EmployeeService employee_service, SystemParameterService? parameter_service = null)
    {
        _attendance_service = attendance_service;
        _employee_service = employee_service;
        _parameter_service = parameter_service;
        Text = "Quản lý chấm công";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(1100, 740);
        MinimumSize = new Size(900, 620);
        AutoScaleMode = AutoScaleMode.Font;

        BuildLayout();
        _status_search.TextChanged += (_, _) => RenderStatuses(SelectedStatusId());
        _record_search.TextChanged += (_, _) => RenderRecords();
        _status_list.SelectedIndexChanged += (_, _) =>
        {
            if (!_rendering)
                ShowSelectedStatus();
        };
        _attendance_list.SelectedIndexChanged += (_, _) =>
        {
            if (!_rendering)
                ShowSelectedRecord();
        };
        Shown += async (_, _) => await RunAsync(() => ReloadAsync());
        FormClosing += (_, args) => args.Cancel = _busy;
    }

    private void BuildLayout()
    {
        _status_list.Columns.Add("Mã", 75);
        _status_list.Columns.Add("Trạng thái", 230);
        _status_list.Columns.Add("Mô tả", 580);

        _attendance_list.Columns.Add("Mã", 70);
        _attendance_list.Columns.Add("Nhân viên", 285);
        _attendance_list.Columns.Add("Ngày", 110);
        _attendance_list.Columns.Add("Giờ vào", 95);
        _attendance_list.Columns.Add("Giờ ra", 95);
        _attendance_list.Columns.Add("Trạng thái khi lưu", 190);

        var status_actions = CreateRow();
        status_actions.Controls.Add(_status_search);
        AddButton(status_actions, "Tải lại", () => ReloadAsync(SelectedStatusId()));
        AddButton(status_actions, "Thêm trạng thái", () =>
        {
            _status_list.SelectedItems.Clear();
            ClearStatusEditor();
            return Task.CompletedTask;
        });
        AddButton(status_actions, "Lưu trạng thái", SaveStatusAsync);
        AddButton(status_actions, "Xóa trạng thái", DeleteStatusAsync);

        var status_fields = CreateRow();
        AddField(status_fields, "Tên trạng thái", _status_name);
        AddField(status_fields, "Mô tả", _status_description);

        var record_actions = CreateRow();
        record_actions.Controls.Add(_record_search);
        AddButton(record_actions, "Thêm chấm công", () =>
        {
            _attendance_list.SelectedItems.Clear();
            ClearRecordEditor();
            return Task.CompletedTask;
        });
        AddButton(record_actions, "Lưu chấm công", SaveRecordAsync);
        AddButton(record_actions, "Xóa chấm công", DeleteRecordAsync);
        record_actions.Controls.Add(_parameter_label);

        var record_fields = CreateRow();
        AddField(record_fields, "Nhân viên", _employee_name);
        AddButton(record_fields, "…", PickEmployeeAsync);
        AddField(record_fields, "Ngày", _work_date);
        AddField(record_fields, "Giờ vào", _check_in);
        AddField(record_fields, "Giờ ra", _check_out);

        _layout.RowStyles.Add(new RowStyle(SizeType.Percent, 40));
        _layout.RowStyles.Add(new RowStyle(SizeType.Percent, 60));
        _layout.Controls.Add(CreateSection("Trạng thái chấm công", status_actions, status_fields, _status_list), 0, 0);
        _layout.Controls.Add(CreateSection("Chấm công theo trạng thái đang chọn", record_actions, record_fields, _attendance_list), 0, 1);
        Controls.Add(_layout);
    }

    private async Task ReloadAsync(int status_id = 0, int record_id = 0)
    {
        if (_parameter_service is not null)
        {
            int work_days = await _parameter_service.GetIntValueAsync("STANDARD_WORK_DAYS", 26);
            decimal work_hours = await _parameter_service.GetDecimalValueAsync("STANDARD_WORK_HOURS", 8m);
            _parameter_label.Text = $"(Công chuẩn: {work_days} ngày/tháng · {work_hours:0.##} giờ/ngày)";
        }

        // Nạp cả hai danh sách trước khi thay dữ liệu đang hiển thị.
        List<AttendanceStatusDto> statuses = await _attendance_service.GetStatusesAsync();
        List<AttendanceDto> records = await _attendance_service.GetRecordsAsync();
        _statuses = new ArrayList(statuses);
        _records = new ArrayList(records);
        RenderStatuses(status_id, record_id);
    }

    private int SelectedStatusId() =>
        _status_list.SelectedItems.Count == 1 &&
        _status_list.SelectedItems[0].Tag is AttendanceStatusDto status ? status.Id : 0;

    private void RenderStatuses(int status_id, int record_id = 0)
    {
        _rendering = true;
        _status_list.BeginUpdate();
        try
        {
            _status_list.Items.Clear();
            ListViewItem? preferred_item = null;
            foreach (AttendanceStatusDto status in _attendance_service.SearchStatuses(_statuses, _status_search.Text))
            {
                var item = new ListViewItem([status.Id.ToString(), status.AttendanceStatus, status.Description]) { Tag = status };
                _status_list.Items.Add(item);
                if (status.Id == status_id)
                    preferred_item = item;
            }

            if (preferred_item is null && _status_list.Items.Count > 0)
                preferred_item = _status_list.Items[0];
            if (preferred_item is not null)
                preferred_item.Selected = true;
        }
        finally
        {
            _status_list.EndUpdate();
            _rendering = false;
        }
        ShowSelectedStatus(record_id);
    }

    private void ShowSelectedStatus(int record_id = 0)
    {
        ClearStatusEditor();
        if (_status_list.SelectedItems.Count == 1 &&
            _status_list.SelectedItems[0].Tag is AttendanceStatusDto status)
        {
            _editing_status_id = status.Id;
            _status_name.Text = status.AttendanceStatus;
            _status_description.Text = status.Description;
        }

        // ListView con chỉ hiển thị các bản ghi thuộc mã của ListView cha.
        RenderRecords(record_id);
    }

    private void RenderRecords(int record_id = 0)
    {
        _rendering = true;
        _attendance_list.BeginUpdate();
        try
        {
            _attendance_list.Items.Clear();
            foreach (AttendanceDto record in _attendance_service.SearchRecords(_records, SelectedStatusId(), _record_search.Text))
            {
                var item = new ListViewItem([
                    record.Id.ToString(),
                    $"{record.EmployeeCode} · {record.EmployeeName}",
                    record.WorkDate.ToString("dd/MM/yyyy"),
                    record.CheckIn.ToString(@"hh\:mm\:ss"),
                    record.CheckOut.ToString(@"hh\:mm\:ss"),
                    record.AttendanceStatus
                ]) { Tag = record };
                _attendance_list.Items.Add(item);
                if (record.Id == record_id)
                    item.Selected = true;
            }
        }
        finally
        {
            _attendance_list.EndUpdate();
            _rendering = false;
        }
        ShowSelectedRecord();
    }

    private void ShowSelectedRecord()
    {
        ClearRecordEditor();
        if (_attendance_list.SelectedItems.Count != 1 ||
            _attendance_list.SelectedItems[0].Tag is not AttendanceDto record)
            return;

        _editing_record_id = record.Id;
        _employee_id = record.EmployeeId;
        _employee_name.Text = $"{record.EmployeeCode} · {record.EmployeeName}";
        _work_date.Value = record.WorkDate;
        _check_in.Value = DateTime.Today.Add(record.CheckIn);
        _check_out.Value = DateTime.Today.Add(record.CheckOut);
    }

    private void ClearStatusEditor()
    {
        _editing_status_id = 0;
        _status_name.Clear();
        _status_description.Clear();
    }

    private void ClearRecordEditor()
    {
        _editing_record_id = 0;
        _employee_id = 0;
        _employee_name.Clear();
        _work_date.Value = DateTime.Today;
        _check_in.Value = DateTime.Today;
        _check_out.Value = DateTime.Today;
    }

    private async Task PickEmployeeAsync()
    {
        using var lookup = new EmployeeLookupForm(_employee_service, await _employee_service.GetAllAsync());
        if (lookup.ShowDialog(this) != DialogResult.OK)
            return;

        EmployeeDto? employee = EmployeeLookupForm.TakeSelection();
        if (employee is null)
            return;

        _employee_id = employee.Id;
        _employee_name.Text = $"{employee.EmployeeCode} · {employee.FullName}";
    }

    private async Task SaveStatusAsync()
    {
        int saved_id = await _attendance_service.SaveStatusAsync(new AttendanceStatusDto
        {
            Id = _editing_status_id,
            AttendanceStatus = _status_name.Text,
            Description = _status_description.Text
        });
        _editing_status_id = saved_id;
        await ReloadAsync(saved_id);
    }

    private async Task SaveRecordAsync()
    {
        int status_id = SelectedStatusId();
        int saved_id = await _attendance_service.SaveRecordAsync(new AttendanceDto
        {
            Id = _editing_record_id,
            EmployeeId = _employee_id,
            AttendanceStatusId = status_id,
            WorkDate = _work_date.Value.Date,
            CheckIn = _check_in.Value.TimeOfDay,
            CheckOut = _check_out.Value.TimeOfDay
        });
        _editing_record_id = saved_id;
        await ReloadAsync(status_id, saved_id);
    }

    private async Task DeleteStatusAsync()
    {
        if (_editing_status_id <= 0)
            throw new ArgumentException("Hãy chọn trạng thái cần xóa.");
        if (!ConfirmDelete("Xóa trạng thái đang chọn?"))
            return;

        await _attendance_service.DeleteStatusAsync(_editing_status_id);
        ClearStatusEditor();
        await ReloadAsync();
    }

    private async Task DeleteRecordAsync()
    {
        if (_editing_record_id <= 0)
            throw new ArgumentException("Hãy chọn bản ghi chấm công cần xóa.");
        if (!ConfirmDelete("Xóa bản ghi chấm công đang chọn?"))
            return;

        await _attendance_service.DeleteRecordAsync(_editing_record_id);
        ClearRecordEditor();
        await ReloadAsync(SelectedStatusId());
    }

    private bool ConfirmDelete(string message) =>
        MessageBox.Show(this, message, "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;

    private void AddButton(FlowLayoutPanel row, string text, Func<Task> action)
    {
        var button = new Button { Text = text, AutoSize = true };
        button.Click += async (_, _) => await RunAsync(action);
        row.Controls.Add(button);
    }

    private async Task RunAsync(Func<Task> action)
    {
        _busy = true;
        _layout.Enabled = false;
        try
        {
            await action();
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
        {
            MessageBox.Show(this, exception.Message, "Kiểm tra dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        catch (Exception)
        {
            MessageBox.Show(this, "Kiểm tra kết nối SQL Server và script 05_SalaryInformation.sql.",
                "Không thể truy cập dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _layout.Enabled = true;
            _busy = false;
        }
    }

    private static ListView CreateListView() => new()
    {
        Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, MultiSelect = false, HideSelection = false
    };

    private static DateTimePicker CreateTimePicker() => new()
    {
        Width = 100, Format = DateTimePickerFormat.Custom, CustomFormat = "HH:mm:ss", ShowUpDown = true
    };

    private static FlowLayoutPanel CreateRow() => new()
    {
        Dock = DockStyle.Fill, AutoSize = true, Padding = new Padding(4)
    };

    private static void AddField(FlowLayoutPanel row, string caption, Control input)
    {
        row.Controls.Add(new Label { Text = caption, AutoSize = true, Margin = new Padding(6, 7, 3, 0) });
        row.Controls.Add(input);
    }

    private static GroupBox CreateSection(string title, FlowLayoutPanel actions, FlowLayoutPanel fields, ListView list)
    {
        var group = new GroupBox { Text = title, Dock = DockStyle.Fill, Padding = new Padding(8) };
        var content = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
        content.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        content.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        content.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        content.Controls.Add(actions, 0, 0);
        content.Controls.Add(fields, 0, 1);
        content.Controls.Add(list, 0, 2);
        group.Controls.Add(content);
        return group;
    }
}

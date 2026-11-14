using DTO.ProjectManagement;

namespace GUI.Forms.ProjectManagement;

// Phần giao diện dùng chung: các form con chỉ khai báo ô nhập và gọi BUS.
public abstract class ManagementFormBase : Form
{
    protected readonly DataGridView Grid = new();
    protected readonly TextBox SearchBox = new();
    protected readonly Panel ExtraToolbarPanel = new() { Dock = DockStyle.Fill, Visible = false };
    protected int SelectedId;
    private readonly TableLayoutPanel _editor = new();
    private readonly TableLayoutPanel _layout = new() { Dock = DockStyle.Fill, RowCount = 5, ColumnCount = 1 };
    private readonly FlowLayoutPanel _toolbar = new();
    private readonly Label _recordLabel = new();
    private readonly Label _messageLabel = new();
    private bool _isLoading;

    protected ManagementFormBase(string title)
    {
        Text = title;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(1180, 760);
        MinimumSize = new Size(960, 640);
        AutoScaleMode = AutoScaleMode.Font;
        Font = new Font("Segoe UI", 10);

        _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 0));
        _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 300));
        _layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
        _toolbar.Dock = DockStyle.Fill;
        _toolbar.Padding = new Padding(8);
        _toolbar.WrapContents = false;
        SearchBox.Width = 240;
        SearchBox.PlaceholderText = "Tìm mã, tên, trạng thái...";
        SearchBox.TextChanged += (_, _) =>
        {
            if (!_isLoading)
            {
                ApplySearch();
                StartNewRecord();
                _messageLabel.Text = $"Đang hiển thị {Grid.Rows.Count} bản ghi.";
            }
        };
        _toolbar.Controls.Add(SearchBox);
        AddButton("Tải lại", async () => await ReloadAsync());
        AddButton("Thêm mới", () => { StartNewRecord(); return Task.CompletedTask; });
        AddButton("Lưu", async () =>
        {
            int id = await SaveRecordAsync();
            // Nếu tải lại bị lỗi, lần lưu sau vẫn cập nhật đúng bản ghi vừa tạo.
            SelectedId = id;
            await ReloadAsync();
            _messageLabel.Text = $"Đã lưu bản ghi mã {id}.";
        });
        AddButton("Xóa", async () =>
        {
            if (SelectedId <= 0)
                throw new ArgumentException("Vui lòng chọn bản ghi cần xóa.");
            if (MessageBox.Show(this, "Xóa bản ghi đang chọn?", "Xác nhận xóa",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            await DeleteRecordAsync();
            await ReloadAsync();
            _messageLabel.Text = "Đã xóa bản ghi.";
        });
        _recordLabel.AutoSize = true;
        _recordLabel.Margin = new Padding(12, 5, 0, 0);
        _toolbar.Controls.Add(_recordLabel);

        _editor.Dock = DockStyle.Fill;
        _editor.AutoScroll = true;
        _editor.ColumnCount = 4;
        _editor.Padding = new Padding(8);
        _editor.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
        _editor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        _editor.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
        _editor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

        Grid.Dock = DockStyle.Fill;
        Grid.ReadOnly = true;
        Grid.AllowUserToAddRows = false;
        Grid.AllowUserToDeleteRows = false;
        Grid.MultiSelect = false;
        Grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        Grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
        Grid.AutoGenerateColumns = false;
        Grid.RowHeadersVisible = false;
        Grid.BackgroundColor = SystemColors.Window;
        Grid.SelectionChanged += (_, _) =>
        {
            if (_isLoading) return;
            if (Grid.CurrentRow?.DataBoundItem == null) return;
            FillInputs();
            _recordLabel.Text = $"Đang sửa mã {SelectedId}";
        };
        _messageLabel.Dock = DockStyle.Fill;
        _messageLabel.Padding = new Padding(8, 4, 0, 0);
        _layout.Controls.Add(_toolbar, 0, 0);
        _layout.Controls.Add(ExtraToolbarPanel, 0, 1);
        _layout.Controls.Add(_editor, 0, 2);
        _layout.Controls.Add(Grid, 0, 3);
        _layout.Controls.Add(_messageLabel, 0, 4);
        Controls.Add(_layout);
        Shown += async (_, _) => await RunAsync(ReloadAsync);
    }

    protected void SetExtraToolbarHeight(int height)
    {
        ExtraToolbarPanel.Visible = height > 0;
        _layout.RowStyles[1] = new RowStyle(SizeType.Absolute, Math.Max(0, height));
    }

    protected void SetStatusMessage(string message) => _messageLabel.Text = message;

    private void AddButton(string text, Func<Task> action)
    {
        var button = new Button { Text = text, AutoSize = true };
        button.Click += async (_, _) => await RunAsync(action);
        _toolbar.Controls.Add(button);
    }

    private async Task RunAsync(Func<Task> action)
    {
        _toolbar.Enabled = false;
        _editor.Enabled = false;
        Grid.Enabled = false;
        UseWaitCursor = true;
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
            MessageBox.Show(this, "Không thể truy cập dữ liệu. Kiểm tra SQL Server, biến HRM_CONNECTION_STRING và script 03_ProjectManagement.sql.",
                "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _toolbar.Enabled = true;
            _editor.Enabled = true;
            Grid.Enabled = true;
            UseWaitCursor = false;
        }
    }

    private async Task ReloadAsync()
    {
        _isLoading = true;
        try
        {
            await LoadRecordsAsync();
            ApplySearch();
            StartNewRecord();
            _messageLabel.Text = $"Đang hiển thị {Grid.Rows.Count} bản ghi.";
        }
        finally
        {
            _isLoading = false;
        }
    }

    private void StartNewRecord()
    {
        SelectedId = 0;
        Grid.ClearSelection();
        Grid.CurrentCell = null;
        ClearInputs();
        _recordLabel.Text = "Đang thêm mới";
    }

    protected void AddField(string label, Control input)
    {
        int index = _editor.Controls.Count / 2;
        int row = index / 2;
        int column = (index % 2) * 2;
        if (row >= _editor.RowCount)
        {
            _editor.RowCount = row + 1;
            _editor.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        }
        _editor.Controls.Add(new Label
        {
            Text = label, AutoSize = true, Anchor = AnchorStyles.Top | AnchorStyles.Left,
            Margin = new Padding(3, 10, 3, 0)
        }, column, row);
        input.Dock = DockStyle.Fill;
        input.Margin = new Padding(3, 6, 12, 6);
        _editor.Controls.Add(input, column + 1, row);
    }

    /// <summary>Đặt ô nhập liệu kèm nút "..." ngay bên cạnh để mở form tra cứu.</summary>
    protected void AddFieldWithLookup(string label, Control input, EventHandler on_lookup_click)
    {
        var container = new TableLayoutPanel
        {
            ColumnCount = 2,
            RowCount = 1,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        container.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        container.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 36));
        container.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        input.Dock = DockStyle.Fill;
        input.Margin = Padding.Empty;
        var lookup_button = new Button
        {
            Text = "...",
            Dock = DockStyle.Fill,
            Margin = new Padding(4, 0, 0, 0)
        };
        lookup_button.Click += on_lookup_click;

        container.Controls.Add(input, 0, 0);
        container.Controls.Add(lookup_button, 1, 0);
        AddField(label, container);
    }

    protected void AddColumn(string property, string header, string? format = null)
    {
        var column = new DataGridViewTextBoxColumn { DataPropertyName = property, HeaderText = header };
        if (format != null) column.DefaultCellStyle.Format = format;
        Grid.Columns.Add(column);
    }

    protected static ComboBox ChoiceBox() => new() { DropDownStyle = ComboBoxStyle.DropDownList };
    protected static DateTimePicker DateBox(bool optional = false) => new()
    {
        Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy", ShowCheckBox = optional
    };
    protected static NumericUpDown NumberBox(decimal maximum) => new()
    {
        Minimum = 0, Maximum = maximum, DecimalPlaces = 2, ThousandsSeparator = true
    };

    protected static void BindChoices(ComboBox box, List<LookupDto> choices)
    {
        box.DisplayMember = nameof(LookupDto.Name);
        box.ValueMember = nameof(LookupDto.Id);
        box.DataSource = choices;
        box.SelectedIndex = -1;
    }

    protected static int ChoiceId(ComboBox box) => box.SelectedValue is int id ? id : 0;
    protected static DateTime? OptionalDate(DateTimePicker box) => box.Checked ? box.Value.Date : null;

    protected static void SetDate(DateTimePicker box, DateTime? value)
    {
        box.Value = value ?? DateTime.Today;
        if (box.ShowCheckBox) box.Checked = value.HasValue;
    }

    protected abstract Task LoadRecordsAsync();
    protected abstract void ApplySearch();
    protected abstract void FillInputs();
    protected abstract void ClearInputs();
    protected abstract Task<int> SaveRecordAsync();
    protected abstract Task DeleteRecordAsync();
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using BUS.HumanResources;
using BUS.HumanResources.Employees;
using DTO.HumanResources.Employees;
namespace GUI.Forms.HumanResources.Employees;
public sealed class EmployeeForm : Form {
    private readonly EmployeeService _service;
    private ArrayList _cachedItems = new();
    private readonly TextBox _txtCode = new() { Location = new Point(140, 20), Width = 180 };
    private readonly TextBox _txtName = new() { Location = new Point(140, 55), Width = 180 };
    private readonly TextBox _txtEmail = new() { Location = new Point(140, 90), Width = 180 };
    private readonly TextBox _txtDeptCode = new() { Location = new Point(450, 20), Width = 140 };
    private readonly TextBox _txtPosCode = new() { Location = new Point(450, 55), Width = 140 };
    private readonly DateTimePicker _dtpHire = new() { Location = new Point(450, 90), Width = 140 };
    private readonly TextBox _txtSearch = new() { Location = new Point(140, 125), Width = 180 };
    private readonly DataGridView _grid = new() { Location = new Point(20, 165), Size = new Size(820, 320), AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false };
    public EmployeeForm(string? conn = null) {
        string c = conn ?? Environment.GetEnvironmentVariable("HRM_CONNECTION_STRING") ?? "Server=localhost;Database=HumanResourceManagement;Integrated Security=True;TrustServerCertificate=True";
        _service = HumanResourcesServiceFactory.CreateEmployeeService(c);
        Text = "Quản lý Nhân viên"; Size = new Size(880, 540);
        Controls.AddRange(new Control[] {
            new Label { Text = "Mã NV (PK):", Location = new Point(20, 23), AutoSize = true }, _txtCode,
            new Label { Text = "Họ tên:", Location = new Point(20, 58), AutoSize = true }, _txtName,
            new Label { Text = "Email:", Location = new Point(20, 93), AutoSize = true }, _txtEmail,
            new Label { Text = "Mã PB:", Location = new Point(360, 23), AutoSize = true }, _txtDeptCode,
            new Label { Text = "Mã CV:", Location = new Point(360, 58), AutoSize = true }, _txtPosCode,
            new Label { Text = "Ngày vào:", Location = new Point(360, 93), AutoSize = true }, _dtpHire,
            new Label { Text = "Tìm kiếm:", Location = new Point(20, 128), AutoSize = true }, _txtSearch, _grid
        });
        var btnLoad = new Button { Text = "Tải lại", Location = new Point(620, 18), Width = 80 };
        var btnSave = new Button { Text = "Lưu", Location = new Point(710, 18), Width = 80 };
        var btnDel = new Button { Text = "Xóa", Location = new Point(620, 55), Width = 80 };
        var btnSearch = new Button { Text = "Tìm", Location = new Point(620, 92), Width = 80 };
        btnLoad.Click += async (_, _) => await LoadDataAsync();
        btnSave.Click += async (_, _) => {
            try {
                await _service.SaveAsync(new EmployeeDto {
                    EmployeeCode = _txtCode.Text, FullName = _txtName.Text, Email = _txtEmail.Text,
                    DepartmentCode = _txtDeptCode.Text, PositionCode = _txtPosCode.Text, HireDate = _dtpHire.Value.Date
                });
                _txtCode.Clear(); _txtName.Clear(); _txtEmail.Clear(); _txtDeptCode.Clear(); _txtPosCode.Clear();
                await LoadDataAsync();
            } catch (Exception ex) { MessageBox.Show(ex.Message); }
        };
        btnDel.Click += async (_, _) => {
            try { await _service.DeleteAsync(_txtCode.Text); _txtCode.Clear(); await LoadDataAsync(); }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        };
        btnSearch.Click += (_, _) => {
            var res = _service.Search(_cachedItems, _txtSearch.Text);
            var l = new List<EmployeeDto>(); foreach (object? o in res) if (o is EmployeeDto d) l.Add(d);
            _grid.DataSource = l;
        };
        _grid.CellClick += (_, e) => {
            if (e.RowIndex >= 0 && _grid.Rows[e.RowIndex].DataBoundItem is EmployeeDto d) {
                _txtCode.Text = d.EmployeeCode; _txtName.Text = d.FullName; _txtEmail.Text = d.Email ?? "";
                _txtDeptCode.Text = d.DepartmentCode; _txtPosCode.Text = d.PositionCode; _dtpHire.Value = d.HireDate;
            }
        };
        Controls.AddRange(new Control[] { btnLoad, btnSave, btnDel, btnSearch });
        Shown += async (_, _) => await LoadDataAsync();
    }
    private async Task LoadDataAsync() {
        var list = await _service.GetAllAsync(); _cachedItems = new ArrayList(list); _grid.DataSource = list;
    }
}

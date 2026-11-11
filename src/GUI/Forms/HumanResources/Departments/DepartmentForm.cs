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
public sealed class DepartmentForm : Form {
    private readonly DepartmentService _service;
    private ArrayList _cachedItems = new();
    private readonly TextBox _txtCode = new() { Location = new Point(140, 20), Width = 200 };
    private readonly TextBox _txtName = new() { Location = new Point(140, 55), Width = 200 };
    private readonly TextBox _txtSearch = new() { Location = new Point(140, 90), Width = 200 };
    private readonly DataGridView _grid = new() { Location = new Point(20, 140), Size = new Size(640, 300), AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false };
    public DepartmentForm(string? conn = null) {
        string c = conn ?? Environment.GetEnvironmentVariable("HRM_CONNECTION_STRING") ?? "Server=localhost;Database=HumanResourceManagement;Integrated Security=True;TrustServerCertificate=True";
        _service = HumanResourcesServiceFactory.CreateDepartmentService(c);
        Text = "Quản lý Phòng ban"; Size = new Size(700, 500);
        Controls.AddRange(new Control[] {
            new Label { Text = "Mã PB (PK):", Location = new Point(20, 23), AutoSize = true }, _txtCode,
            new Label { Text = "Tên PB:", Location = new Point(20, 58), AutoSize = true }, _txtName,
            new Label { Text = "Tìm kiếm:", Location = new Point(20, 93), AutoSize = true }, _txtSearch, _grid
        });
        var btnLoad = new Button { Text = "Tải lại", Location = new Point(370, 18), Width = 80 };
        var btnSave = new Button { Text = "Lưu", Location = new Point(460, 18), Width = 80 };
        var btnDel = new Button { Text = "Xóa", Location = new Point(550, 18), Width = 80 };
        var btnSearch = new Button { Text = "Tìm", Location = new Point(370, 88), Width = 80 };
        btnLoad.Click += async (_, _) => await LoadDataAsync();
        btnSave.Click += async (_, _) => {
            try { await _service.SaveAsync(new DepartmentDto { Code = _txtCode.Text, Name = _txtName.Text }); _txtCode.Clear(); _txtName.Clear(); await LoadDataAsync(); }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        };
        btnDel.Click += async (_, _) => {
            try { await _service.DeleteAsync(_txtCode.Text); _txtCode.Clear(); _txtName.Clear(); await LoadDataAsync(); }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        };
        btnSearch.Click += (_, _) => {
            var res = _service.Search(_cachedItems, _txtSearch.Text);
            var l = new List<DepartmentDto>(); foreach (object? o in res) if (o is DepartmentDto d) l.Add(d);
            _grid.DataSource = l;
        };
        _grid.CellClick += (_, e) => {
            if (e.RowIndex >= 0 && _grid.Rows[e.RowIndex].DataBoundItem is DepartmentDto d) { _txtCode.Text = d.Code; _txtName.Text = d.Name; }
        };
        Controls.AddRange(new Control[] { btnLoad, btnSave, btnDel, btnSearch });
        Shown += async (_, _) => await LoadDataAsync();
    }
    private async Task LoadDataAsync() {
        var list = await _service.GetAllAsync(); _cachedItems = new ArrayList(list); _grid.DataSource = list;
    }
}

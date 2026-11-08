using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using BUS.HumanResources;
using BUS.HumanResources.Contracts;
using DTO.HumanResources.Contracts;
namespace GUI.Forms.HumanResources.Contracts;
public sealed class ContractForm : Form {
    private readonly ContractService _service;
    private ArrayList _cachedItems = new();
    private int _selectedId;
    private readonly TextBox _txtCode = new() { Location = new Point(140, 20), Width = 180 };
    private readonly TextBox _txtEmpId = new() { Location = new Point(140, 55), Width = 180 };
    private readonly TextBox _txtSalary = new() { Location = new Point(140, 90), Width = 180 };
    private readonly DateTimePicker _dtpStart = new() { Location = new Point(450, 20), Width = 140 };
    private readonly DateTimePicker _dtpEnd = new() { Location = new Point(450, 55), Width = 140, ShowCheckBox = true };
    private readonly TextBox _txtSearch = new() { Location = new Point(450, 90), Width = 140 };
    private readonly DataGridView _grid = new() { Location = new Point(20, 145), Size = new Size(780, 310), AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false };
    public ContractForm(string? conn = null) {
        string c = conn ?? Environment.GetEnvironmentVariable("HRM_CONNECTION_STRING") ?? "Server=localhost;Database=HumanResourceManagement;Integrated Security=True;TrustServerCertificate=True";
        _service = HumanResourcesServiceFactory.CreateContractService(c);
        Text = "Quản lý Hợp đồng"; Size = new Size(840, 510);
        Controls.AddRange(new Control[] {
            new Label { Text = "Mã HĐ:", Location = new Point(20, 23), AutoSize = true }, _txtCode,
            new Label { Text = "ID NV:", Location = new Point(20, 58), AutoSize = true }, _txtEmpId,
            new Label { Text = "Lương CB:", Location = new Point(20, 93), AutoSize = true }, _txtSalary,
            new Label { Text = "Ngày bắt đầu:", Location = new Point(340, 23), AutoSize = true }, _dtpStart,
            new Label { Text = "Ngày kết thúc:", Location = new Point(340, 58), AutoSize = true }, _dtpEnd,
            new Label { Text = "Tìm kiếm:", Location = new Point(340, 93), AutoSize = true }, _txtSearch, _grid
        });
        var btnLoad = new Button { Text = "Tải lại", Location = new Point(620, 18), Width = 80 };
        var btnSave = new Button { Text = "Lưu", Location = new Point(710, 18), Width = 80 };
        var btnDel = new Button { Text = "Xóa", Location = new Point(620, 55), Width = 80 };
        var btnSearch = new Button { Text = "Tìm", Location = new Point(620, 90), Width = 80 };
        btnLoad.Click += async (_, _) => await LoadDataAsync();
        btnSave.Click += async (_, _) => {
            try {
                int.TryParse(_txtEmpId.Text.Trim(), out int eId);
                decimal.TryParse(_txtSalary.Text.Trim(), out decimal sal);
                await _service.SaveAsync(new ContractDto {
                    Id = _selectedId, EmployeeId = eId, ContractCode = _txtCode.Text, StartDate = _dtpStart.Value.Date,
                    EndDate = _dtpEnd.Checked ? _dtpEnd.Value.Date : null, BaseSalary = sal
                });
                _selectedId = 0; _txtCode.Clear(); _txtEmpId.Clear(); _txtSalary.Clear();
                await LoadDataAsync();
            } catch (Exception ex) { MessageBox.Show(ex.Message); }
        };
        btnDel.Click += async (_, _) => {
            try { await _service.DeleteAsync(_selectedId); _selectedId = 0; await LoadDataAsync(); }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        };
        btnSearch.Click += (_, _) => {
            var res = _service.Search(_cachedItems, _txtSearch.Text);
            var l = new List<ContractDto>(); foreach (object? o in res) if (o is ContractDto d) l.Add(d);
            _grid.DataSource = l;
        };
        _grid.CellClick += (_, e) => {
            if (e.RowIndex >= 0 && _grid.Rows[e.RowIndex].DataBoundItem is ContractDto d) {
                _selectedId = d.Id; _txtCode.Text = d.ContractCode; _txtEmpId.Text = d.EmployeeId.ToString(); _txtSalary.Text = d.BaseSalary.ToString("0.##");
                _dtpStart.Value = d.StartDate; if (d.EndDate.HasValue) { _dtpEnd.Checked = true; _dtpEnd.Value = d.EndDate.Value; } else _dtpEnd.Checked = false;
            }
        };
        Controls.AddRange(new Control[] { btnLoad, btnSave, btnDel, btnSearch });
        Shown += async (_, _) => await LoadDataAsync();
    }
    private async Task LoadDataAsync() {
        var list = await _service.GetAllAsync(); _cachedItems = new ArrayList(list); _grid.DataSource = list;
    }
}

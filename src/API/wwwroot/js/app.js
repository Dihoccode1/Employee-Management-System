import { getEmployees } from './services/employees-api.js';

const button = document.querySelector('#load-employees');
const status = document.querySelector('#status');
const body = document.querySelector('#employees-body');

button.addEventListener('click', async () => {
  button.disabled = true;
  status.textContent = 'Đang tải…';
  body.replaceChildren();
  try {
    const employees = await getEmployees();
    for (const employee of employees) {
      const row = document.createElement('tr');
      for (const value of [employee.employeeCode, employee.fullName, employee.email ?? '', employee.isActive ? 'Đang làm việc' : 'Đã nghỉ']) {
        const cell = document.createElement('td');
        cell.textContent = value;
        row.append(cell);
      }
      body.append(row);
    }
    status.textContent = `Có ${employees.length} nhân viên.`;
  } catch (error) {
    status.textContent = error.message;
  } finally {
    button.disabled = false;
  }
});

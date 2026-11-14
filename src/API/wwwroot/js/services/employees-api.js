export async function getEmployees() {
  const response = await fetch('/api/employees', { headers: { Accept: 'application/json' } });
  if (!response.ok) {
    const problem = await response.json().catch(() => null);
    throw new Error(problem?.title || 'Không thể tải nhân viên. Kiểm tra kết nối SQL Server.');
  }
  return response.json();
}

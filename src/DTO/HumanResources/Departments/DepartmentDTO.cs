namespace HRM.DTO
{
    public class DepartmentDTO
    {
        public string departmentId { get; set; }
        public string departmentName { get; set; }
        public string description { get; set; }

        public DepartmentDTO() { }

        public DepartmentDTO(string departmentId, string departmentName, string description)
        {
            this.departmentId = departmentId;
            this.departmentName = departmentName;
            this.description = description;
        }
    }
}

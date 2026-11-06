namespace DTO.ProjectManagement.Projects;

public class ProjectDto
{
    public int id { get; set; }
    public string name { get; set; } = "";
    public DateTime startDate { get; set; }
    public DateTime? endDate { get; set; }
    public DateTime deadline { get; set; }
    public string description { get; set; } = "";
    public decimal budget { get; set; }
    public int departmentId { get; set; }
    public int managerId { get; set; }
    public string status { get; set; } = "Not started";
}

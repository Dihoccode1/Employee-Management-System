namespace DTO.ProjectManagement.ProjectDetails;

public class ProjectDetailDto
{
    public int id { get; set; }
    public int projectId { get; set; }
    public decimal completionRate { get; set; }
    public string status { get; set; } = "Not started";
    public decimal allowance { get; set; }
    public decimal reward { get; set; }
}

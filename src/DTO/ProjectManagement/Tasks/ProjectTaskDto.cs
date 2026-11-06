namespace DTO.ProjectManagement.Tasks;

public class ProjectTaskDto
{
    public int id { get; set; }
    public int projectDetailId { get; set; }
    public string name { get; set; } = "";
    public string description { get; set; } = "";
    public string taskType { get; set; } = "";
    public string priority { get; set; } = "Normal";
    public DateTime startDate { get; set; }
    public DateTime deadline { get; set; }
    public DateTime? completedDate { get; set; }
    public decimal estimatedHours { get; set; }
    public decimal completionRate { get; set; }
    public string status { get; set; } = "Not started";
    public string notes { get; set; } = "";
}

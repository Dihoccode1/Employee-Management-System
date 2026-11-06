namespace DTO.ProjectManagement.Assignments;

public class AssignmentDto
{
    public int id { get; set; }
    public int taskId { get; set; }
    public int projectId { get; set; }
    public int employeeId { get; set; }
    public DateTime assignedDate { get; set; }
    public DateTime startDate { get; set; }
    public DateTime deadline { get; set; }
    public DateTime? completedDate { get; set; }
    public decimal completionRate { get; set; }
    public decimal contributionScore { get; set; }
    public string status { get; set; } = "Not started";
    public string notes { get; set; } = "";
}

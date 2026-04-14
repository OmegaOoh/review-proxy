namespace Issue.Models.Dtos;

public class IssuePatchRequest
{
    public string? Title { get; set; }
    public string? Body { get; set; }
    public IssueStatus? Status { get; set; }
}

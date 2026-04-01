namespace Repository.Models.Dtos;

public class DepositRequest
{
    public string GithubRepoId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

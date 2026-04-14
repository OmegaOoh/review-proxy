namespace Identity.Interfaces.Services;

public interface ITokenService
{
    string IssueJwt(Guid userId, string username);
}

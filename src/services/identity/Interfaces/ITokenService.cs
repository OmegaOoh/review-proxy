namespace Identity.Interfaces;

public interface ITokenService
{
    string IssueJwt(Guid userId, string username);
}

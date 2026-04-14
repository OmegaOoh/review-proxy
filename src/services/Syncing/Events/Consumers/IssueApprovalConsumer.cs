using MassTransit;
using ReviewProxy.Contracts;
using Syncing.Interfaces.Services;

namespace Syncing.Events.Consumers;

public class IssueApprovalConsumer(ISyncingService syncingService, ILogger<IssueApprovalConsumer> logger) : IConsumer<IssueApprovalEvent>
{
    public async Task Consume(ConsumeContext<IssueApprovalEvent> context)
    {
        logger.LogInformation("Processing IssueApprovalEvent for Issue {IssueId} in Repo {GitHubRepoId}",
            context.Message.IssueId, context.Message.GitHubRepoId);

        try
        {
            await syncingService.SyncIssueToGitHubAsync(context.Message);
            logger.LogInformation("Successfully synced Issue {IssueId} to GitHub", context.Message.IssueId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to sync Issue {IssueId} to GitHub", context.Message.IssueId);
            throw;
        }
    }
}

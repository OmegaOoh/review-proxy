using ReviewProxy.Contracts;

namespace Issue.Interfaces.Events;

public interface IIssueEventPublisher
{
    Task PublishIssueApprovalAsync(IssueApprovalEvent approvalEvent);
}

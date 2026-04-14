using Issue.Interfaces.Events;
using MassTransit;
using ReviewProxy.Contracts;

namespace Issue.Events.Producers;

public class IssueEventPublisher(IPublishEndpoint publishEndpoint) : IIssueEventPublisher
{
    public async Task PublishIssueApprovalAsync(IssueApprovalEvent approvalEvent)
    {
        await publishEndpoint.Publish(approvalEvent);
    }
}

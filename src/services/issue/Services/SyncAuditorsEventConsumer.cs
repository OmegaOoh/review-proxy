namespace Issue.Services;

using Issue.Data;
using Issue.Interfaces;
using MassTransit;
using ReviewProxy.Contracts;

public class SyncAuditorsEventConsumer(
    IssueDbContext dbContext,
    ILogger<SyncAuditorsEventConsumer> logger) : IConsumer<SyncAuditorListEvent>
{
    public async Task Consume(ConsumeContext<SyncAuditorListEvent> context)
    {
        var eventData = context.Message;

        // Structured logging at INFO level
        logger.LogInformation("Received SyncAuditorListEvent. MessageId: {MessageId}.",
            context.MessageId);

        // Your logic here
        await Task.CompletedTask;
    }
}

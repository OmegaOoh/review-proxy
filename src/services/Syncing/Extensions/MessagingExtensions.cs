using MassTransit;
using ReviewProxy.Contracts;
using Syncing.Events.Consumers;

namespace Syncing.Extensions;

public static class MessagingExtensions
{
    public static IServiceCollection AddSyncingMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(options =>
        {
            options.AddConsumer<IssueApprovalConsumer>();

            options.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(new Uri(configuration["MassTransit:Host"] ?? "rabbitmq://localhost:5672"), h =>
                {
                    h.Username(configuration["MassTransit:Username"] ?? "guest");
                    h.Password(configuration["MassTransit:Password"] ?? "guest");
                });

                cfg.Message<IssueApprovalEvent>(e => e.SetEntityName("issue-approval-exchange"));

                cfg.ReceiveEndpoint("syncing-service-issue-approval", e =>
                {
                    e.ConfigureConsumer<IssueApprovalConsumer>(context);
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}

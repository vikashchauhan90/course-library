using Azure.Messaging.ServiceBus;
using CourseLibrary.Domain.Events;
using CourseLibrary.EventConsumer.Configuration.Observability.Traces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CourseLibrary.EventConsumer.Consumers.Authors.CreateAuthor;

internal class CreateAuthorConsumer(
    DurableTaskClient durableTaskClient,
    ILogger<CreateAuthorConsumer> logger)
{
    [Function("CreateAuthorConsumer")]
    public async Task RunAsync(
       [ServiceBusTrigger(
            "%CreateAuthorEventsQueue%",
            Connection = "ServiceBusConnection")]
        ServiceBusReceivedMessage message,
       CancellationToken cancellationToken)
    {
        var parentContext =
     ServiceBusTraceContext.Extract(message);

        using var activity =
            ActivitySources.EventConsumer.StartActivity(
                "author.event.process",
                ActivityKind.Consumer,
                parentContext);

        logger.LogInformation(
            "Processing Service Bus message {MessageId}.",
            message.MessageId);


        var authorEvent =
            message.Body.ToObjectFromJson<AuthorCreatedEvent>();

        var instanceId =
           $"author-created-{message.MessageId}";

        var existingInstance =
            await durableTaskClient.GetInstanceAsync(
                instanceId,
                cancellationToken);

        if (existingInstance is not null)
        {
            logger.LogWarning(
                "Message {MessageId} has already been scheduled with orchestration instance {InstanceId}.",
                message.MessageId,
                instanceId);

            return;
        }

        await durableTaskClient.ScheduleNewOrchestrationInstanceAsync(
             nameof(CreateAuthorOrchestrator),
             authorEvent,
             new StartOrchestrationOptions
             {
                 InstanceId = instanceId
             },
             cancellationToken);
    }
}

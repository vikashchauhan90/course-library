using Azure;
using Azure.Messaging.ServiceBus;
using CourseLibrary.Application.Abstractions.Idempotency;
using CourseLibrary.Application.Abstractions.Serialization;
using CourseLibrary.Application.Abstractions.Serializers;
using CourseLibrary.Domain.Events;
using CourseLibrary.EventConsumer.Configuration;
using CourseLibrary.EventConsumer.Configuration.Observability.Traces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CourseLibrary.EventConsumer.Consumers.Authors.CreateAuthor;

internal class CreateAuthorConsumer(
    ISerializerFactory serializerFactory,
    ILogger<CreateAuthorConsumer> logger)
{
    private readonly ISerializer<AuthorCreatedEvent> _serializer =
       serializerFactory.Create<AuthorCreatedEvent>(
           SerializerType.MessagePack);
    [Function("CreateAuthorConsumer")]
    public async Task RunAsync(
       [ServiceBusTrigger(
            "AuthorCreated",
        "CreateAuthorConsumer",
        Connection = "ServiceBusConnection")]
        ServiceBusReceivedMessage message,
        [DurableClient] DurableTaskClient durableTaskClient,
       CancellationToken cancellationToken)
    {
        var parentContext =
     ServiceBusTraceContext.Extract(message);

        using var activity =
            ActivitySources.EventConsumer.StartActivity(
                "author.event.process",
                ActivityKind.Consumer,
                parentContext);

        // Set consumer-level tags
        activity?.SetTag("messaging.system", "servicebus");
        activity?.SetTag("messaging.destination", "CreateAuthorEventsQueue");
        activity?.SetTag("messaging.message_id", message.MessageId);
        activity?.SetTag("messaging.correlation_id", message.CorrelationId);
        activity?.SetTag("messaging.delivery_count", message.DeliveryCount);
        activity?.SetTag("request.traceId", ServiceBusTraceContext.GetProperty(message, "TraceId"));

        logger.LogInformation(
            "Processing Service Bus message {MessageId}.",
            message.MessageId);

        AuthorCreatedEvent? authorEvent = _serializer.Deserialize(message.Body.ToArray());


        if (authorEvent is null)
        {
            logger.LogWarning(
                "Received message {MessageId} with invalid author event.",
                message.MessageId);

            activity?.SetStatus(ActivityStatusCode.Error, "Invalid author event.");

            return;
        }

        var instanceId =
           $"author-created-{message.MessageId}";

        var existingInstance =
            await durableTaskClient.GetInstanceAsync(
                instanceId,
                cancellationToken);

        activity?.SetTag("orchestration.instance_id", instanceId);

        if (existingInstance is not null)
        {
            logger.LogWarning(
                "Message {MessageId} has already been scheduled with orchestration instance {InstanceId}.",
                message.MessageId,
                instanceId);

            activity?.SetTag("orchestration.exists", true);
            activity?.SetStatus(ActivityStatusCode.Ok);

            return;
        }

        activity?.SetTag("orchestration.exists", false);
        activity?.SetStatus(ActivityStatusCode.Ok);

        var parentTraceContext = RequestTraceContextFactory.FromActivity(activity);

        // CHILD ACTIVITY: Schedule orchestration
        using (var scheduleActivity = ActivitySources.EventConsumer.StartActivity(
            "consumer.schedule.orchestration",
            ActivityKind.Client,
            RequestTraceContextFactory.ToActivityContext(parentTraceContext)))
        {
            scheduleActivity?.SetTag("orchestration.name", nameof(CreateAuthorOrchestrator));
            scheduleActivity?.SetTag("orchestration.instance_id", instanceId);
            scheduleActivity?.SetTag("event.author_id", authorEvent.AuthorId);
            try
            {
                // Link to the new orchestration (this will be the parent for the orchestration)
                var links = new List<ActivityLink>();

                if (activity != null)
                {
                    // Create link to the consumer activity
                    links.Add(new ActivityLink(activity.Context));
                }

                await durableTaskClient.ScheduleNewOrchestrationInstanceAsync(
                nameof(CreateAuthorOrchestrator),
                new DurableTraceInput<AuthorCreatedEvent>(
                    Data: authorEvent,
                    TraceContext: parentTraceContext
                ),
                new StartOrchestrationOptions
                {
                    InstanceId = instanceId
                },
                cancellationToken);

                scheduleActivity?.SetTag("orchestration.scheduled", true);
                scheduleActivity?.SetStatus(ActivityStatusCode.Ok);

                logger.LogInformation(
                    "Scheduled orchestration {InstanceId} for AuthorId {AuthorId}",
                    instanceId,
                    authorEvent.AuthorId);
            }
            catch (Exception ex)
            {
                scheduleActivity?.SetStatus(ActivityStatusCode.Error, ex.Message);

                logger.LogError(
                    ex,
                    "Failed to schedule orchestration {InstanceId} for AuthorId {AuthorId}",
                    instanceId,
                    authorEvent.AuthorId);

                throw;
            }
        }
        // Set final status on consumer activity
        activity?.SetStatus(ActivityStatusCode.Ok);

        logger.LogInformation(
            "Successfully processed message {MessageId} for AuthorId {AuthorId}",
            message.MessageId,
            authorEvent.AuthorId);

    }
}

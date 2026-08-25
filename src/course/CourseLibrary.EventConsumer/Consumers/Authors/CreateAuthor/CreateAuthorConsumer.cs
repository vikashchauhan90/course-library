using Azure.Messaging.ServiceBus;
using CourseLibrary.Application.Abstractions.Serialization;
using CourseLibrary.Application.Abstractions.Serializers;
using CourseLibrary.Domain.Events;
using CourseLibrary.EventConsumer.Configuration.Observability.Metrics;
using CourseLibrary.EventConsumer.Configuration.Observability.Traces;
using DurableTask.Core.History;
using Google.Protobuf.WellKnownTypes;
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
        var propagationContext = 
            CourseLibrary.Infrastructure.Observability.Traces.ServiceBusTraceContext.Extract(message);

        using var activity = ActivitySources.EventConsumer.StartActivity(
            "author.event.process",
            ActivityKind.Consumer,
            propagationContext.ActivityContext);

        try
        {
            // Restore baggage
            if (activity != null)
            {
                foreach (var item in propagationContext.Baggage.GetBaggage())
                {
                    activity.AddBaggage(item.Key, item.Value);
                }
            }

            // Set consumer-level tags
            activity?.SetTag("messaging.system", "servicebus");
            activity?.SetTag("messaging.destination", "CreateAuthorEventsQueue");
            activity?.SetTag("messaging.message_id", message.MessageId);
            activity?.SetTag("messaging.correlation_id", message.CorrelationId);
            activity?.SetTag("messaging.delivery_count", message.DeliveryCount);
            activity?.SetTag("request.traceId", activity?.TraceId.ToString());

            logger.LogInformation("Processing Service Bus message {MessageId}.", message.MessageId);

            Meters.RecordMessageConsumed(
           "AuthorCreated",
           message.MessageId,
           "AuthorCreated");

            var authorEvent = _serializer.Deserialize(message.Body.ToArray());

            if (authorEvent is null)
            {

                logger.LogWarning("Received message {MessageId} with invalid author event.", message.MessageId);
                activity?.SetStatus(ActivityStatusCode.Error, "Invalid author event.");
                return;
            }

            var instanceId = $"author-created-{message.MessageId}";
            var existingInstance = await durableTaskClient.GetInstanceAsync(instanceId, cancellationToken);

            activity?.SetTag("orchestration.instance_id", instanceId);
            activity?.SetTag("orchestration.exists", existingInstance is not null);

            if (existingInstance is not null)
            {
                logger.LogWarning(
                    "Message {MessageId} has already been scheduled with orchestration instance {InstanceId}.",
                    message.MessageId, instanceId);
                return;
            }

            // Schedule orchestration as child activity
            using (var scheduleActivity = ActivitySources.EventConsumer.StartActivity(
                "consumer.schedule.orchestration",
                ActivityKind.Client))
            {
                scheduleActivity?.SetTag("orchestration.name", nameof(CreateAuthorOrchestrator));
                scheduleActivity?.SetTag("orchestration.instance_id", instanceId);
                scheduleActivity?.SetTag("event.author_id", authorEvent.AuthorId);

                try
                {
                    await durableTaskClient.ScheduleNewOrchestrationInstanceAsync(
                        nameof(CreateAuthorOrchestrator),
                        authorEvent,
                        new StartOrchestrationOptions { InstanceId = instanceId },
                        cancellationToken);

                    scheduleActivity?.SetTag("orchestration.scheduled", true);
                    scheduleActivity?.SetStatus(ActivityStatusCode.Ok);

                    logger.LogInformation(
                        "Scheduled orchestration {InstanceId} for AuthorId {AuthorId}",
                        instanceId, authorEvent.AuthorId);
                }
                catch (Exception ex)
                {
                    scheduleActivity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    logger.LogError(ex, "Failed to schedule orchestration {InstanceId}", instanceId);
                    throw;
                }
            }

            activity?.SetStatus(ActivityStatusCode.Ok);
            logger.LogInformation(
                "Successfully processed message {MessageId} for AuthorId {AuthorId}",
                message.MessageId, authorEvent.AuthorId);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            logger.LogError(ex, "Error processing message {MessageId}", message.MessageId);
            throw;
        }
    }
}

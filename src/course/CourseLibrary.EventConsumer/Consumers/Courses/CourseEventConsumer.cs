using Azure.Messaging.ServiceBus;
using CourseLibrary.Application.Abstractions.Serialization;
using CourseLibrary.Application.Abstractions.Serializers;
using CourseLibrary.Domain.Events;
using CourseLibrary.EventConsumer.Configuration.Observability.Metrics;
using CourseLibrary.EventConsumer.Configuration.Observability.Traces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CourseLibrary.EventConsumer.Consumers.Courses;

internal sealed class CourseEventConsumer(
    ISerializerFactory serializerFactory,
    ILogger<CourseEventConsumer> logger)
{
    private readonly ISerializer<CourseEvent> serializer = serializerFactory.Create<CourseEvent>(SerializerType.MessagePack);

    [Function(nameof(CourseEventConsumer))]
    public async Task RunAsync(
        [ServiceBusTrigger(
        "CourseEvent",
        "CourseEventConsumer",
        Connection = "ServiceBusConnection")] ServiceBusReceivedMessage message,
        [DurableClient] DurableTaskClient durableTaskClient,
        CancellationToken cancellationToken)
    {
        var started = Stopwatch.GetTimestamp();
        var propagationContext = CourseLibrary.Infrastructure.Observability.Traces.ServiceBusTraceContext.Extract(message);
        using var activity = ActivitySources.EventConsumer.StartActivity("course.event.process", ActivityKind.Consumer, propagationContext.ActivityContext);
        try
        {
            activity?.SetTag("messaging.system", "servicebus");
            activity?.SetTag("messaging.message_id", message.MessageId);
            activity?.SetTag("messaging.delivery_count", message.DeliveryCount);

            var courseEvent = serializer.Deserialize(message.Body.ToArray());
            if (courseEvent is null)
            {
                Meters.DeserializationFailures.Add(1, new TagList { { "event_type", "CourseEvent" } });
                activity?.SetStatus(ActivityStatusCode.Error, "Invalid event payload");
                logger.LogWarning("Received invalid CourseEvent message {MessageId}.", message.MessageId);
                return;
            }

            var courseEventType = courseEvent.EventType.ToString();

            activity?.SetTag("messaging.destination", courseEventType);
            Meters.RecordMessageConsumed("CourseEvent", message.MessageId, courseEventType);
            var instanceId = $"course-created-{message.MessageId}";
            if (await durableTaskClient.GetInstanceAsync(instanceId, cancellationToken) is not null)
            {
                Meters.DuplicateMessagesDetected.Add(1, new TagList { { "event_type", "CourseEvent" } });
                logger.LogWarning("CourseEvent message {MessageId} was already scheduled.", message.MessageId);
                return;
            }

            await durableTaskClient.ScheduleNewOrchestrationInstanceAsync(
                nameof(CourseEventOrchestrator), courseEvent,
                new StartOrchestrationOptions { InstanceId = instanceId }, cancellationToken);
            Meters.RecordOrchestrationStarted(nameof(CourseEventOrchestrator), instanceId);
            activity?.SetStatus(ActivityStatusCode.Ok);
            Meters.RecordMessageProcessed("CourseEvent", message.MessageId, courseEventType, Stopwatch.GetElapsedTime(started).TotalMilliseconds);
        }
        catch (Exception exception)
        {
            activity?.SetStatus(ActivityStatusCode.Error, exception.GetType().Name);
            Meters.RecordMessageFailed("CourseEvent", message.MessageId, "CourseEvent", exception);
            logger.LogError(exception, "Failed to process CourseEvent message {MessageId}.", message.MessageId);
            throw;
        }
    }
}

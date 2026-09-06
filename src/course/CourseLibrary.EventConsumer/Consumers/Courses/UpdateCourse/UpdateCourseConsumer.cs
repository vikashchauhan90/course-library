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

namespace CourseLibrary.EventConsumer.Consumers.Courses.UpdateCourse;

internal sealed class UpdateCourseConsumer(ISerializerFactory serializerFactory, ILogger<UpdateCourseConsumer> logger)
{
    private readonly ISerializer<CourseUpdatedEvent> serializer = serializerFactory.Create<CourseUpdatedEvent>(SerializerType.MessagePack);

    [Function(nameof(UpdateCourseConsumer))]
    public async Task RunAsync(
        [ServiceBusTrigger("CourseUpdated", "UpdateCourseConsumer", Connection = "ServiceBusConnection")] ServiceBusReceivedMessage message,
        [DurableClient] DurableTaskClient durableTaskClient,
        CancellationToken cancellationToken)
    {
        var started = Stopwatch.GetTimestamp();
        var propagationContext = CourseLibrary.Infrastructure.Observability.Traces.ServiceBusTraceContext.Extract(message);
        using var activity = ActivitySources.EventConsumer.StartActivity("course.event.process", ActivityKind.Consumer, propagationContext.ActivityContext);
        try
        {
            activity?.SetTag("messaging.system", "servicebus");
            activity?.SetTag("messaging.destination", "CourseUpdated");
            activity?.SetTag("messaging.message_id", message.MessageId);
            activity?.SetTag("messaging.delivery_count", message.DeliveryCount);
            var courseEvent = serializer.Deserialize(message.Body.ToArray());
            if (courseEvent is null)
            {
                Meters.DeserializationFailures.Add(1, new TagList { { "event_type", "CourseUpdated" } });
                activity?.SetStatus(ActivityStatusCode.Error, "Invalid event payload");
                logger.LogWarning("Received invalid CourseUpdated message {MessageId}.", message.MessageId);
                return;
            }
            Meters.RecordMessageConsumed("CourseUpdated", message.MessageId, "CourseUpdated");
            var instanceId = $"course-updated-{message.MessageId}";
            if (await durableTaskClient.GetInstanceAsync(instanceId, cancellationToken) is not null)
            {
                Meters.DuplicateMessagesDetected.Add(1, new TagList { { "event_type", "CourseUpdated" } });
                logger.LogWarning("CourseUpdated message {MessageId} was already scheduled.", message.MessageId);
                return;
            }
            await durableTaskClient.ScheduleNewOrchestrationInstanceAsync(nameof(UpdateCourseOrchestrator), courseEvent, new StartOrchestrationOptions { InstanceId = instanceId }, cancellationToken);
            Meters.RecordOrchestrationStarted(nameof(UpdateCourseOrchestrator), instanceId);
            activity?.SetStatus(ActivityStatusCode.Ok);
            Meters.RecordMessageProcessed("CourseUpdated", message.MessageId, "CourseUpdated", Stopwatch.GetElapsedTime(started).TotalMilliseconds);
        }
        catch (Exception exception)
        {
            activity?.SetStatus(ActivityStatusCode.Error, exception.GetType().Name);
            Meters.RecordMessageFailed("CourseUpdated", message.MessageId, "CourseUpdated", exception);
            logger.LogError(exception, "Failed to process CourseUpdated message {MessageId}.", message.MessageId);
            throw;
        }
    }
}

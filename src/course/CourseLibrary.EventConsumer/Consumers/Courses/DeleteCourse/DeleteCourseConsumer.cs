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

namespace CourseLibrary.EventConsumer.Consumers.Courses.DeleteCourse;

internal sealed class DeleteCourseConsumer(ISerializerFactory serializerFactory, ILogger<DeleteCourseConsumer> logger)
{
    private readonly ISerializer<CourseDeletedEvent> serializer = serializerFactory.Create<CourseDeletedEvent>(SerializerType.MessagePack);

    [Function(nameof(DeleteCourseConsumer))]
    public async Task RunAsync(
        [ServiceBusTrigger("CourseDeleted", "DeleteCourseConsumer", Connection = "ServiceBusConnection")] ServiceBusReceivedMessage message,
        [DurableClient] DurableTaskClient durableTaskClient,
        CancellationToken cancellationToken)
    {
        var started = Stopwatch.GetTimestamp();
        var propagationContext = CourseLibrary.Infrastructure.Observability.Traces.ServiceBusTraceContext.Extract(message);
        using var activity = ActivitySources.EventConsumer.StartActivity("course.event.process", ActivityKind.Consumer, propagationContext.ActivityContext);
        try
        {
            activity?.SetTag("messaging.system", "servicebus");
            activity?.SetTag("messaging.destination", "CourseDeleted");
            activity?.SetTag("messaging.message_id", message.MessageId);
            activity?.SetTag("messaging.delivery_count", message.DeliveryCount);
            var courseEvent = serializer.Deserialize(message.Body.ToArray());
            if (courseEvent is null)
            {
                Meters.DeserializationFailures.Add(1, new TagList { { "event_type", "CourseDeleted" } });
                activity?.SetStatus(ActivityStatusCode.Error, "Invalid event payload");
                logger.LogWarning("Received invalid CourseDeleted message {MessageId}.", message.MessageId);
                return;
            }
            Meters.RecordMessageConsumed("CourseDeleted", message.MessageId, "CourseDeleted");
            var instanceId = $"course-deleted-{message.MessageId}";
            if (await durableTaskClient.GetInstanceAsync(instanceId, cancellationToken) is not null)
            {
                Meters.DuplicateMessagesDetected.Add(1, new TagList { { "event_type", "CourseDeleted" } });
                logger.LogWarning("CourseDeleted message {MessageId} was already scheduled.", message.MessageId);
                return;
            }
            await durableTaskClient.ScheduleNewOrchestrationInstanceAsync(nameof(DeleteCourseOrchestrator), courseEvent, new StartOrchestrationOptions { InstanceId = instanceId }, cancellationToken);
            Meters.RecordOrchestrationStarted(nameof(DeleteCourseOrchestrator), instanceId);
            activity?.SetStatus(ActivityStatusCode.Ok);
            Meters.RecordMessageProcessed("CourseDeleted", message.MessageId, "CourseDeleted", Stopwatch.GetElapsedTime(started).TotalMilliseconds);
        }
        catch (Exception exception)
        {
            activity?.SetStatus(ActivityStatusCode.Error, exception.GetType().Name);
            Meters.RecordMessageFailed("CourseDeleted", message.MessageId, "CourseDeleted", exception);
            logger.LogError(exception, "Failed to process CourseDeleted message {MessageId}.", message.MessageId);
            throw;
        }
    }
}

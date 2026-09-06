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

namespace CourseLibrary.EventConsumer.Consumers.Courses.CreateCourse;

internal sealed class CreateCourseConsumer(ISerializerFactory serializerFactory, ILogger<CreateCourseConsumer> logger)
{
    private readonly ISerializer<CourseCreatedEvent> serializer = serializerFactory.Create<CourseCreatedEvent>(SerializerType.MessagePack);

    [Function(nameof(CreateCourseConsumer))]
    public async Task RunAsync(
        [ServiceBusTrigger("CourseCreated", "CreateCourseConsumer", Connection = "ServiceBusConnection")] ServiceBusReceivedMessage message,
        [DurableClient] DurableTaskClient durableTaskClient,
        CancellationToken cancellationToken)
    {
        var started = Stopwatch.GetTimestamp();
        var propagationContext = CourseLibrary.Infrastructure.Observability.Traces.ServiceBusTraceContext.Extract(message);
        using var activity = ActivitySources.EventConsumer.StartActivity("course.event.process", ActivityKind.Consumer, propagationContext.ActivityContext);
        try
        {
            activity?.SetTag("messaging.system", "servicebus");
            activity?.SetTag("messaging.destination", "CourseCreated");
            activity?.SetTag("messaging.message_id", message.MessageId);
            activity?.SetTag("messaging.delivery_count", message.DeliveryCount);

            var courseEvent = serializer.Deserialize(message.Body.ToArray());
            if (courseEvent is null)
            {
                Meters.DeserializationFailures.Add(1, new TagList { { "event_type", "CourseCreated" } });
                activity?.SetStatus(ActivityStatusCode.Error, "Invalid event payload");
                logger.LogWarning("Received invalid CourseCreated message {MessageId}.", message.MessageId);
                return;
            }

            Meters.RecordMessageConsumed("CourseCreated", message.MessageId, "CourseCreated");
            var instanceId = $"course-created-{message.MessageId}";
            if (await durableTaskClient.GetInstanceAsync(instanceId, cancellationToken) is not null)
            {
                Meters.DuplicateMessagesDetected.Add(1, new TagList { { "event_type", "CourseCreated" } });
                logger.LogWarning("CourseCreated message {MessageId} was already scheduled.", message.MessageId);
                return;
            }

            await durableTaskClient.ScheduleNewOrchestrationInstanceAsync(
                nameof(CreateCourseOrchestrator), courseEvent,
                new StartOrchestrationOptions { InstanceId = instanceId }, cancellationToken);
            Meters.RecordOrchestrationStarted(nameof(CreateCourseOrchestrator), instanceId);
            activity?.SetStatus(ActivityStatusCode.Ok);
            Meters.RecordMessageProcessed("CourseCreated", message.MessageId, "CourseCreated", Stopwatch.GetElapsedTime(started).TotalMilliseconds);
        }
        catch (Exception exception)
        {
            activity?.SetStatus(ActivityStatusCode.Error, exception.GetType().Name);
            Meters.RecordMessageFailed("CourseCreated", message.MessageId, "CourseCreated", exception);
            logger.LogError(exception, "Failed to process CourseCreated message {MessageId}.", message.MessageId);
            throw;
        }
    }
}

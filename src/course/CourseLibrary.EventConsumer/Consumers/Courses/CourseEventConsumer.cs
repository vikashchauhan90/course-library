using Azure.Messaging.ServiceBus;
using CourseLibrary.Application.Abstractions.Serialization;
using CourseLibrary.Application.Abstractions.Serializers;
using CourseLibrary.Domain.Events;
using CourseLibrary.EventConsumer.Configuration.Observability.Metrics;
using CourseLibrary.EventConsumer.Configuration.Observability.Traces;
using CourseLibrary.EventConsumer.Core;
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
        Connection = "ServiceBusConnection", AutoCompleteMessages = false)] ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions,
        [DurableClient] DurableTaskClient durableTaskClient,
        CancellationToken cancellationToken)
    {
        var started = Stopwatch.GetTimestamp();
        var propagationContext =
            Infrastructure.Observability.Traces.ServiceBusTraceContext.Extract(message);

        using var activity = ActivitySources.StartActivity(
            "course.event.process",
            ActivityKind.Consumer,
            propagationContext.ActivityContext);
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

            var existingInstance =
                await durableTaskClient.GetInstanceAsync(
                    instanceId,
                    cancellationToken);

            if (OrchestrationState.IsActiveInstance(existingInstance))
            {
                Meters.DuplicateMessagesDetected.Add(
                            1,
                            new TagList {
                                { "event_type", "CourseEvent" },
                                { "instance_id", instanceId },
                                { "status", existingInstance?.RuntimeStatus.ToString() }
                            });

                logger.LogWarning(
                    "CourseEvent message {MessageId} already has active orchestration {InstanceId} with status {Status}.",
                    message.MessageId,
                    instanceId,
                    existingInstance?.RuntimeStatus);

                return;
            }
            if (OrchestrationState.IsStaleInstance(existingInstance))
            {
                logger.LogInformation(
                           "CourseEvent orchestration {InstanceId} is stale with status {Status}. Purging before starting a new instance.",
                           instanceId,
                           existingInstance?.RuntimeStatus);

                await durableTaskClient.PurgeInstanceAsync(
                    instanceId,
                    cancellationToken);
            }


            var orchestrationInput = new OrchestrationInput<CourseEvent>
            {
                Event = courseEvent,
                ParentContext = propagationContext.ActivityContext
            };

            await durableTaskClient.ScheduleNewOrchestrationInstanceAsync(
                nameof(CourseEventOrchestrator),
                orchestrationInput,
                new StartOrchestrationOptions
                {
                    InstanceId = instanceId,
                    StartAt = DateTimeOffset.UtcNow,
                },
                cancellationToken);

            Meters.RecordOrchestrationStarted(
                nameof(CourseEventOrchestrator),
                instanceId);

            activity?.SetStatus(ActivityStatusCode.Ok);

            Meters.RecordMessageProcessed(
                "CourseEvent",
                message.MessageId,
                courseEventType,
                Stopwatch.GetElapsedTime(started).TotalMilliseconds);

            await messageActions.CompleteMessageAsync(message, cancellationToken);
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

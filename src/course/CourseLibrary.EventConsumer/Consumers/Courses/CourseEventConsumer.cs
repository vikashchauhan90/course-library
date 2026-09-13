using Azure.Messaging.ServiceBus;
using CourseLibrary.Application.Abstractions.Idempotency;
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
using InfraTraces = CourseLibrary.Infrastructure.Observability.Traces;

namespace CourseLibrary.EventConsumer.Consumers.Courses;

internal sealed class CourseEventConsumer(
    ISerializerFactory serializerFactory,
    IIdempotencyStore idempotencyStore,
    ILogger<CourseEventConsumer> logger)
{
    private readonly ISerializer<CourseEvent> serializer = serializerFactory.Create<CourseEvent>(SerializerType.Json);
    private static readonly byte[] ConsumedMarker = [1];
    public static string GetIdempotencyKey(string messageId, string eventId) => $"event:message:{messageId}:{eventId}";

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
            InfraTraces.ServiceBusTraceContext.Extract(message);

        using var scope = logger.BeginScope(
            new Dictionary<string, object?>
            {
                ["messaging.system"] = "servicebus",
                ["messaging.destination"] = "CourseEvent",
                ["messaging.operation"] = "process",
                ["messaging.message_id"] = message.MessageId,
                ["messaging.correlation_id"] = message.CorrelationId,
                ["messaging.delivery_count"] = message.DeliveryCount
            });

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
                const string deadLetterReason = "InvalidCourseEvent";
                const string deadLetterDescription =
                    "The message body could not be deserialized as a CourseEvent.";

                activity?.SetTag("messaging.destination", "CourseEvent/$DeadLetterQueue");
                activity?.SetTag("messaging.dead_letter.reason", deadLetterReason);
                activity?.SetStatus(ActivityStatusCode.Error, deadLetterReason);

                logger.LogWarning(
                    "Dead-lettering invalid CourseEvent message {MessageId} with reason {DeadLetterReason}.",
                    message.MessageId,
                    deadLetterReason);

                await messageActions.DeadLetterMessageAsync(
                    message,
                    propertiesToModify: null,
                    deadLetterReason: deadLetterReason,
                    deadLetterErrorDescription: deadLetterDescription,
                    cancellationToken: cancellationToken);

                Meters.ServiceBusMessagesDeadLettered.Add(
                    1,
                    new TagList
                    {
                        { "event_type", "CourseEvent" },
                        { "reason", deadLetterReason }
                    });

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

            var messageConsumedKey = GetIdempotencyKey(message.MessageId, courseEvent.EventId);
            var cacheResult = await idempotencyStore.GetOrCreateAsync(
                messageConsumedKey,
                async ct => IdempotencyEntry.Empty,
                ttl: TimeSpan.FromSeconds(1),
                tags: ["event-consumed"],
                cancellationToken: cancellationToken);
            if (!cacheResult.IsEmpty)
            {
                logger.LogWarning("Event already consumed. MessageId: {MessageId}, EventId: {EventId}, IdempotencyKey: {IdempotencyKey}. Completing the message without reprocessing.",
                    message.MessageId,
                    courseEvent.EventId,
                    messageConsumedKey);

                await messageActions.CompleteMessageAsync(message, cancellationToken);
                return;
            }

            var orchestrationInput = new OrchestrationInput<CourseEvent>
            {
                Event = courseEvent,
                ParentTraceParent =
                InfraTraces.ServiceBusTraceContext.GetTraceParent(message),
                ParentTraceState =
                InfraTraces.ServiceBusTraceContext.GetTraceState(message)
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

            await idempotencyStore.StoreAsync(
                messageConsumedKey,
                IdempotencyEntry.GetIdempotencyEntry(ConsumedMarker),
                ttl: TimeSpan.FromMinutes(5),
                tags: ["event-consumed"],
                cancellationToken: cancellationToken
                );

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

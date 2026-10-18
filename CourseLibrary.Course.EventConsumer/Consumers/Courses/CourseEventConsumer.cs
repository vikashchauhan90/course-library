using Azure.Messaging.ServiceBus;
using CourseLibrary.Course.Domain.Events;
using CourseLibrary.Course.EventConsumer.Core;
using CourseLibrary.Course.EventConsumer.Observability;
using CourseLibrary.Course.EventConsumer.RateLimiting;
using CourseLibrary.Platform.Messaging.Abstractions;
using CourseLibrary.Platform.Messaging.Observability;
using CourseLibrary.Platform.Observability.Traces;
using CourseLibrary.Platform.Serialization.Abstractions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CourseLibrary.Course.EventConsumer.Consumers.Courses;

internal sealed class CourseEventConsumer(
    ISerializerFactory serializerFactory,
    CourseEventRateLimiter rateLimiter,
    ILogger<CourseEventConsumer> logger)
{
    private const string TopicName = "CourseEvent";
    private const string SubscriptionName = "CourseEventConsumer";
    private const string EventTypeName = nameof(CourseEvent);

    private readonly ISerializer<CourseEvent> _serializer =
        serializerFactory.Create<CourseEvent>(SerializerType.Json);

    [Function(nameof(CourseEventConsumer))]
    public async Task RunAsync(
        [ServiceBusTrigger(
            TopicName,
            SubscriptionName,
            Connection = "ServiceBusConnection",
            AutoCompleteMessages = true)]
        ServiceBusReceivedMessage message,
        [DurableClient] DurableTaskClient durableTaskClient,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(durableTaskClient);

        var startedAt = Stopwatch.GetTimestamp();
        var propagationContext = ServiceBusTraceContext.Extract(message);
        var eventMetadata = ServiceBusTraceContext.CreateMetadata(
            message,
            propagationContext);
        var parentContext = propagationContext.ActivityContext;
        if (parentContext.TraceId == default
            && ActivityContext.TryParse(
                eventMetadata.TraceParent,
                eventMetadata.TraceState,
                isRemote: true,
                out var remoteContext))
        {
            parentContext = remoteContext;
        }

        using var scope = logger.BeginScope(new Dictionary<string, object?>
        {
            [CourseEventConsumerActivityTagConstants.MessagingSystem] = "servicebus",
            [CourseEventConsumerActivityTagConstants.MessagingDestination] = TopicName,
            [CourseEventConsumerActivityTagConstants.MessagingDestinationKind] = "topic",
            [CourseEventConsumerActivityTagConstants.MessagingOperation] = "process",
            [CourseEventConsumerActivityTagConstants.MessagingMessageId] = message.MessageId,
            [CourseEventConsumerActivityTagConstants.MessagingCorrelationId] = message.CorrelationId,
            [CourseEventConsumerActivityTagConstants.MessagingDeliveryCount] = message.DeliveryCount,
            [CourseEventConsumerActivityTagConstants.MessagingEventId] = eventMetadata.Properties.TryGetValue(
                ServiceBusTraceContext.EventId,
                out var eventId)
                    ? eventId
                    : null,
            [CourseEventConsumerActivityTagConstants.MessagingEventType] = eventMetadata.Properties.TryGetValue(
                ServiceBusTraceContext.EventType,
                out var eventType)
                    ? eventType
                    : null
        });

        using var activity = CourseEventConsumerActivitySource.Shared.StartActivity(
            "course.event.process",
            ActivityKind.Consumer,
            parentContext.TraceId == default ? null : parentContext);

        try
        {
            await rateLimiter.WaitForMessagePermitAsync(cancellationToken);

            foreach (var item in eventMetadata.Baggage)
            {
                activity?.AddBaggage(item.Key, item.Value);
            }

            SetActivityTags(activity, message);
            if (eventMetadata.Properties.TryGetValue(
                    ServiceBusTraceContext.EventId,
                    out var metadataEventId))
            {
                activity?.SetTag(
                    CourseEventConsumerActivityTagConstants.MessagingEventId,
                    metadataEventId);
            }

            if (eventMetadata.Properties.TryGetValue(
                    ServiceBusTraceContext.EventType,
                    out var metadataEventType))
            {
                activity?.SetTag(
                    CourseEventConsumerActivityTagConstants.MessagingEventType,
                    metadataEventType);
            }

            CourseEvent? courseEvent;
            try
            {
                courseEvent = _serializer.Deserialize(message.Body.ToArray());
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                throw new InvalidOperationException(
                    "The message body could not be deserialized as a CourseEvent.",
                    exception);
            }

            if (courseEvent is null)
            {
                throw new InvalidOperationException(
                    "The message body deserialized to null and could not be processed.");
            }

            activity?.SetTag(
                CourseEventConsumerActivityTagConstants.MessagingEventType,
                courseEvent.EventKind.ToString());

            var instanceId = $"course-event-{message.MessageId}";
            var existingInstance = await durableTaskClient.GetInstanceAsync(
                instanceId,
                cancellationToken);

            if (OrchestrationState.IsActiveInstance(existingInstance))
            {
                CourseEventConsumerMeter.Shared.RecordDuplicate(
                    existingInstance!.RuntimeStatus.ToString());
                logger.LogWarning(
                    "Duplicate CourseEvent message {MessageId}; orchestration {InstanceId} is {Status}.",
                    message.MessageId,
                    instanceId,
                    existingInstance.RuntimeStatus);
                activity?.SetTag(CourseEventConsumerActivityTagConstants.MessagingDuplicate, true);
                activity?.SetStatus(ActivityStatusCode.Ok, "Duplicate message");
                CourseEventConsumerMeter.Shared.RecordMessage(
                    "duplicate",
                    Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds);
                return;
            }

            if (OrchestrationState.IsStaleInstance(existingInstance))
            {
                logger.LogInformation(
                    "Purging stale CourseEvent orchestration {InstanceId} with status {Status}.",
                    instanceId,
                    existingInstance?.RuntimeStatus);
                await durableTaskClient.PurgeInstanceAsync(instanceId, cancellationToken);
            }

            var orchestrationInput = new OrchestrationInput<CourseEventPayload>
            {
                Event = CourseEventPayload.FromDomain(courseEvent),
                MessageId = message.MessageId,
                Metadata = eventMetadata
            };

            try
            {
                await durableTaskClient.ScheduleNewOrchestrationInstanceAsync(
                    nameof(CourseEventOrchestrator),
                    orchestrationInput,
                    new StartOrchestrationOptions
                    {
                        InstanceId = instanceId,
                        StartAt = DateTimeOffset.UtcNow
                    },
                    cancellationToken);
            }
            catch (Exception exception) when (IsDuplicateOrchestrationException(exception))
            {
                CourseEventConsumerMeter.Shared.RecordDuplicate("AlreadyExists");
                activity?.SetTag(CourseEventConsumerActivityTagConstants.MessagingDuplicate, true);
                activity?.SetStatus(ActivityStatusCode.Ok, "Duplicate orchestration");
                logger.LogWarning(
                    "CourseEvent message {MessageId} lost the orchestration creation race for {InstanceId}.",
                    message.MessageId,
                    instanceId);
                CourseEventConsumerMeter.Shared.RecordMessage(
                    "duplicate",
                    Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds);
                return;
            }

            CourseEventConsumerMeter.Shared.RecordOrchestrationStarted(
                nameof(CourseEventOrchestrator));
            activity?.SetStatus(ActivityStatusCode.Ok);
            CourseEventConsumerMeter.Shared.RecordMessage(
                "success",
                Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds);
            logger.LogInformation(
                "Started CourseEvent orchestration {InstanceId} for message {MessageId}.",
                instanceId,
                message.MessageId);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            activity?.SetTag(CourseEventConsumerActivityTagConstants.MessagingCancelled, true);
            CourseEventConsumerMeter.Shared.RecordFailure(
                "process",
                "cancelled");
            CourseEventConsumerMeter.Shared.RecordMessage(
                "cancelled",
                Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds);
            throw;
        }
        catch (Exception exception)
        {
            ActivityEnrichmentHelper.RecordException(activity, exception);
            CourseEventConsumerMeter.Shared.RecordFailure(
                "process",
                exception.GetType().Name);
            CourseEventConsumerMeter.Shared.RecordMessage(
                "failure",
                Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds);
            logger.LogError(
                exception,
                "Failed to process CourseEvent message {MessageId}.",
                message.MessageId);
            throw;
        }
    }

    private static void SetActivityTags(
        Activity? activity,
        ServiceBusReceivedMessage message)
    {
        activity?.SetTag(CourseEventConsumerActivityTagConstants.MessagingSystem, "servicebus");
        activity?.SetTag(CourseEventConsumerActivityTagConstants.MessagingMessageId, message.MessageId);
        activity?.SetTag(CourseEventConsumerActivityTagConstants.MessagingCorrelationId, message.CorrelationId);
        activity?.SetTag(CourseEventConsumerActivityTagConstants.MessagingDeliveryCount, message.DeliveryCount);
        activity?.SetTag(CourseEventConsumerActivityTagConstants.MessagingDestination, TopicName);
        activity?.SetTag(CourseEventConsumerActivityTagConstants.MessagingDestinationKind, "topic");
    }

    private static bool IsDuplicateOrchestrationException(Exception exception)
        => exception.Message.Contains("already exists", StringComparison.OrdinalIgnoreCase)
           || exception.Message.Contains("already exist", StringComparison.OrdinalIgnoreCase)
           || exception.Message.Contains("duplicate", StringComparison.OrdinalIgnoreCase);
}

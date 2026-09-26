using Azure.Messaging.ServiceBus;
using CourseLibrary.Application.Abstractions.Serialization;
using CourseLibrary.Application.Abstractions.Serializers;
using CourseLibrary.Domain.Events;
using CourseLibrary.EventConsumer.Configuration.Observability.Metrics;
using CourseLibrary.EventConsumer.Configuration.Observability.Traces;
using CourseLibrary.EventConsumer.Core;
using CourseLibrary.Infrastructure.Configuration.Messaging;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

using InfraTraces =
    CourseLibrary.Infrastructure.Observability.Traces;

namespace CourseLibrary.EventConsumer.Consumers.Courses;

internal sealed class CourseEventConsumer(
    ISerializerFactory serializerFactory,
    ILogger<CourseEventConsumer> logger)
{
    private const string TopicName = "CourseEvent";
    private const string SubscriptionName = "CourseEventConsumer";
    private const string EventTypeName = "CourseEvent";

    private readonly ISerializer<CourseEvent> serializer =
        serializerFactory.Create<CourseEvent>(SerializerType.Json);

    [Function(nameof(CourseEventConsumer))]
    public async Task RunAsync(
        [ServiceBusTrigger(
            TopicName,
            SubscriptionName,
            Connection = "ServiceBusConnection",
            AutoCompleteMessages = false)]
        ServiceBusReceivedMessage message,
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
                ["messaging.destination"] = TopicName,
                ["messaging.destination_kind"] = "topic",
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
            logger.LogInformation(
         "Current trace context: TraceId={TraceId}, SpanId={SpanId}, ParentSpanId={ParentSpanId}",
         Activity.Current?.TraceId.ToString(),
         Activity.Current?.SpanId.ToString(),
         Activity.Current?.ParentSpanId.ToString());

            SetActivityTags(activity, message);

            CourseEvent? courseEvent;

            try
            {
                courseEvent = serializer.Deserialize(
                    message.Body.ToArray());
            }
            catch (Exception exception) when (
                exception is not OperationCanceledException)
            {
                await DeadLetterInvalidMessageAsync(
                    message,
                    messageActions,
                    activity,
                    "CourseEventDeserializationFailed",
                    "The message body could not be deserialized as a CourseEvent.",
                    exception,
                    cancellationToken);

                return;
            }

            if (courseEvent is null)
            {
                await DeadLetterInvalidMessageAsync(
                    message,
                    messageActions,
                    activity,
                    "InvalidCourseEvent",
                    "The message body deserialized to null and could not be processed.",
                    exception: null,
                    cancellationToken);

                return;
            }

            var courseEventType =
                courseEvent.EventType.ToString();

            activity?.SetTag(
                "messaging.event.type",
                courseEventType);

            Meters.RecordMessageConsumed(
                TopicName,
                message.MessageId,
                courseEventType);

            //
            // 2. Build deterministic orchestration ID.
            //
            var instanceId =
                $"course-created-{message.MessageId}";

            //
            // 3. Check for an existing orchestration.
            //
            var existingInstance =
                await durableTaskClient.GetInstanceAsync(
                    instanceId,
                    getInputsAndOutputs: false,
                    cancellationToken);

            //
            // 4. Active instance means duplicate delivery.
            //
            if (OrchestrationState.IsActiveInstance(existingInstance))
            {
                Meters.DuplicateMessagesDetected.Add(
                    1,
                    new TagList
                    {
                        { "event_type", EventTypeName },
                        { "instance_id", instanceId },
                        {
                            "status",
                            existingInstance!.RuntimeStatus.ToString()
                        }
                    });

                logger.LogWarning(
                    "Duplicate CourseEvent message {MessageId}. " +
                    "Orchestration {InstanceId} already exists with status {Status}.",
                    message.MessageId,
                    instanceId,
                    existingInstance.RuntimeStatus);

                activity?.SetTag(
                    "messaging.duplicate",
                    true);

                activity?.SetStatus(
                    ActivityStatusCode.Ok,
                    "Duplicate message");

                //
                // The existing orchestration owns this event.
                // The Service Bus message can now be removed.
                //
                await CompleteMessageAsync(
                    message,
                    messageActions,
                    cancellationToken);

                return;
            }

            //
            // 5. Terminal/stale instance.
            //
            if (OrchestrationState.IsStaleInstance(existingInstance))
            {
                logger.LogInformation(
                    "Purging stale CourseEvent orchestration {InstanceId} " +
                    "with status {Status}.",
                    instanceId,
                    existingInstance?.RuntimeStatus);

                await durableTaskClient.PurgeInstanceAsync(
                    instanceId,
                    cancellationToken);
            }

            //
            // 6. Create orchestration input.
            //
            var orchestrationInput =
                new OrchestrationInput<CourseEventPayload>
                {
                    Event =
                        CourseEventPayload.FromDomain(courseEvent),

                    MessageId =
                        message.MessageId,

                    Metadata =
                        ServiceBusEventMetadata.FromMessage(message),
                };

            //
            // 7. Schedule using deterministic InstanceId.
            //
            // This is the actual deduplication boundary.
            //
            try
            {
                await durableTaskClient
                    .ScheduleNewOrchestrationInstanceAsync(
                        nameof(CourseEventOrchestrator),
                        orchestrationInput,
                        new StartOrchestrationOptions
                        {
                            InstanceId = instanceId,
                            StartAt = DateTimeOffset.UtcNow
                        },
                        cancellationToken);
            }
            catch (Exception exception) when (
                IsDuplicateOrchestrationException(exception))
            {
                //
                // Race:
                //
                // Consumer A -> GetInstance == null
                // Consumer B -> GetInstance == null
                // Consumer A -> Schedule succeeds
                // Consumer B -> Schedule sees same InstanceId
                //
                // B is a duplicate delivery, not a processing failure.
                //
                Meters.DuplicateMessagesDetected.Add(
                    1,
                    new TagList
                    {
                        { "event_type", EventTypeName },
                        { "instance_id", instanceId },
                        { "status", "AlreadyExists" }
                    });

                logger.LogWarning(
                    "CourseEvent message {MessageId} lost the orchestration " +
                    "creation race for {InstanceId}. Treating as duplicate.",
                    message.MessageId,
                    instanceId);

                activity?.SetTag(
                    "messaging.duplicate",
                    true);

                activity?.SetStatus(
                    ActivityStatusCode.Ok,
                    "Duplicate orchestration");

                await CompleteMessageAsync(
                    message,
                    messageActions,
                    cancellationToken);

                return;
            }

            //
            // 8. Durable Task accepted the orchestration.
            //
            Meters.RecordOrchestrationStarted(
                nameof(CourseEventOrchestrator),
                instanceId);

            logger.LogInformation(
                "Started CourseEvent orchestration {InstanceId} " +
                "for message {MessageId}.",
                instanceId,
                message.MessageId);

            activity?.SetStatus(ActivityStatusCode.Ok);

            Meters.RecordMessageProcessed(
                TopicName,
                message.MessageId,
                courseEventType,
                Stopwatch.GetElapsedTime(started).TotalMilliseconds);

            //
            // 9. ONLY NOW complete the Service Bus message.
            //
            await CompleteMessageAsync(
                message,
                messageActions,
                cancellationToken);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            //
            // Do not complete the message.
            // Azure Functions / Service Bus can retry it.
            //
            throw;
        }
        catch (Exception exception)
        {
            activity?.SetStatus(
                ActivityStatusCode.Error,
                exception.GetType().Name);

            Meters.RecordMessageFailed(
                TopicName,
                message.MessageId,
                EventTypeName,
                exception);

            logger.LogError(
                exception,
                "Failed to process CourseEvent message {MessageId}.",
                message.MessageId);

            //
            // IMPORTANT:
            // Do not complete or dead-letter transient/unclassified
            // processing failures here.
            //
            // Throwing allows Service Bus retry / max delivery count
            // to eventually move the message to the DLQ.
            //
            throw;
        }
    }

    private async Task DeadLetterInvalidMessageAsync(
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions,
        Activity? activity,
        string reason,
        string description,
        Exception? exception,
        CancellationToken cancellationToken)
    {
        Meters.DeserializationFailures.Add(
            1,
            new TagList
            {
                { "event_type", EventTypeName },
                { "reason", reason }
            });

        activity?.SetTag(
            "messaging.destination",
            $"{TopicName}/$DeadLetterQueue");

        activity?.SetTag(
            "messaging.dead_letter.reason",
            reason);

        activity?.SetStatus(
            ActivityStatusCode.Error,
            reason);

        if (exception is not null)
        {
            logger.LogWarning(
                exception,
                "Dead-lettering invalid CourseEvent message {MessageId}. " +
                "Reason: {DeadLetterReason}.",
                message.MessageId,
                reason);
        }
        else
        {
            logger.LogWarning(
                "Dead-lettering invalid CourseEvent message {MessageId}. " +
                "Reason: {DeadLetterReason}.",
                message.MessageId,
                reason);
        }

        await messageActions.DeadLetterMessageAsync(
            message,
            propertiesToModify: null,
            deadLetterReason: reason,
            deadLetterErrorDescription: description,
            cancellationToken: cancellationToken);

        Meters.ServiceBusMessagesDeadLettered.Add(
            1,
            new TagList
            {
                { "event_type", EventTypeName },
                { "reason", reason }
            });
    }

    private static async Task CompleteMessageAsync(
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions,
        CancellationToken cancellationToken)
    {
        await messageActions.CompleteMessageAsync(
            message,
            cancellationToken);
    }

    private static void SetActivityTags(
        Activity? activity,
        ServiceBusReceivedMessage message)
    {
        activity?.SetTag(
            "messaging.system",
            "servicebus");

        activity?.SetTag(
            "messaging.message_id",
            message.MessageId);

        activity?.SetTag(
            "messaging.correlation_id",
            message.CorrelationId);

        activity?.SetTag(
            "messaging.delivery_count",
            message.DeliveryCount);

        activity?.SetTag(
            "messaging.destination",
            TopicName);

        activity?.SetTag(
            "messaging.destination_kind",
            "topic");
    }

    private static bool IsDuplicateOrchestrationException(
        Exception exception)
    {
        //
        // Keep this method isolated because the exact exception exposed
        // by the configured Durable Task backend can vary.
        //
        // If your provider exposes a specific "already exists" exception,
        // check that type here instead of relying on message text.
        //
        var message =
            exception.Message;

        return message.Contains(
                   "already exists",
                   StringComparison.OrdinalIgnoreCase)
               ||
               message.Contains(
                   "already exist",
                   StringComparison.OrdinalIgnoreCase)
               ||
               message.Contains(
                   "duplicate",
                   StringComparison.OrdinalIgnoreCase);
    }
}
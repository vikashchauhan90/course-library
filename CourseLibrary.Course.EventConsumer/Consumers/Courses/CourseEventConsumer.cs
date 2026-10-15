using Azure.Messaging.ServiceBus;
using CourseLibrary.Course.Domain.Events;
using CourseLibrary.Course.EventConsumer.Core;
using CourseLibrary.Course.EventConsumer.Observability;
using CourseLibrary.Platform.Messaging.Abstractions;
using CourseLibrary.Platform.Messaging.Observability;
using CourseLibrary.Platform.Serialization.Abstractions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CourseLibrary.Course.EventConsumer.Consumers.Courses;

internal sealed class CourseEventConsumer(
    ISerializerFactory serializerFactory,
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
            AutoCompleteMessages = false)]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions,
        [DurableClient] DurableTaskClient durableTaskClient,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(messageActions);
        ArgumentNullException.ThrowIfNull(durableTaskClient);

        var startedAt = Stopwatch.GetTimestamp();
        var traceParent = GetTraceProperty(message, ServiceBusTraceContext.TraceParent)
            ?? GetTraceProperty(message, ServiceBusTraceContext.DiagnosticId);
        var traceState = GetTraceProperty(message, ServiceBusTraceContext.TraceState);
        ActivityContext? parentContext = null;
        if (ActivityContext.TryParse(traceParent, traceState, true, out var remoteContext))
        {
            parentContext = remoteContext;
        }
        else
        {
            var extractedContext = ServiceBusTraceContext.Extract(message).ActivityContext;
            if (extractedContext.TraceId != default)
            {
                parentContext = extractedContext;
            }
        }

        using var scope = logger.BeginScope(new Dictionary<string, object?>
        {
            ["messaging.system"] = "servicebus",
            ["messaging.destination"] = TopicName,
            ["messaging.destination_kind"] = "topic",
            ["messaging.operation"] = "process",
            ["messaging.message_id"] = message.MessageId,
            ["messaging.correlation_id"] = message.CorrelationId,
            ["messaging.delivery_count"] = message.DeliveryCount
        });

        using var activity = CourseEventConsumerActivitySource.Shared.StartActivity(
            "course.event.process",
            ActivityKind.Consumer,
            parentContext);

        try
        {
            SetActivityTags(activity, message);

            CourseEvent? courseEvent;
            try
            {
                courseEvent = _serializer.Deserialize(message.Body.ToArray());
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                await DeadLetterInvalidMessageAsync(
                    message,
                    messageActions,
                    activity,
                    "CourseEventDeserializationFailed",
                    "The message body could not be deserialized as a CourseEvent.",
                    exception,
                    cancellationToken);
                CourseEventConsumerMeter.Shared.RecordMessage(
                    "dead_lettered",
                    Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds);
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
                    null,
                    cancellationToken);
                CourseEventConsumerMeter.Shared.RecordMessage(
                    "dead_lettered",
                    Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds);
                return;
            }

            activity?.SetTag("messaging.event.type", courseEvent.EventKind.ToString());

            var instanceId = $"course-event-{message.MessageId}";
            var existingInstance = await durableTaskClient.GetInstanceAsync(
                instanceId,
                getInputsAndOutputs: false,
                cancellationToken: cancellationToken);

            if (OrchestrationState.IsActiveInstance(existingInstance))
            {
                CourseEventConsumerMeter.Shared.RecordDuplicate(
                    existingInstance!.RuntimeStatus.ToString());
                logger.LogWarning(
                    "Duplicate CourseEvent message {MessageId}; orchestration {InstanceId} is {Status}.",
                    message.MessageId,
                    instanceId,
                    existingInstance.RuntimeStatus);
                activity?.SetTag("messaging.duplicate", true);
                activity?.SetStatus(ActivityStatusCode.Ok, "Duplicate message");
                await CompleteMessageAsync(message, messageActions, cancellationToken);
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
                Metadata = new EventMetadata
                {
                    TraceParent = traceParent,
                    TraceState = traceState
                }
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
                activity?.SetTag("messaging.duplicate", true);
                activity?.SetStatus(ActivityStatusCode.Ok, "Duplicate orchestration");
                logger.LogWarning(
                    "CourseEvent message {MessageId} lost the orchestration creation race for {InstanceId}.",
                    message.MessageId,
                    instanceId);
                await CompleteMessageAsync(message, messageActions, cancellationToken);
                CourseEventConsumerMeter.Shared.RecordMessage(
                    "duplicate",
                    Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds);
                return;
            }

            CourseEventConsumerMeter.Shared.RecordOrchestrationStarted(
                nameof(CourseEventOrchestrator));
            activity?.SetStatus(ActivityStatusCode.Ok);
            await CompleteMessageAsync(message, messageActions, cancellationToken);
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
            CourseEventConsumerMeter.Shared.RecordFailure(
                "process",
                "cancelled");
            throw;
        }
        catch (Exception exception)
        {
            activity?.SetStatus(ActivityStatusCode.Error, exception.GetType().Name);
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

    private async Task DeadLetterInvalidMessageAsync(
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions,
        Activity? activity,
        string reason,
        string description,
        Exception? exception,
        CancellationToken cancellationToken)
    {
        activity?.SetTag("messaging.destination", $"{TopicName}/$DeadLetterQueue");
        activity?.SetTag("messaging.dead_letter.reason", reason);
        activity?.SetStatus(ActivityStatusCode.Error, reason);

        if (exception is null)
        {
            logger.LogWarning(
                "Dead-lettering invalid CourseEvent message {MessageId}. Reason: {Reason}.",
                message.MessageId,
                reason);
        }
        else
        {
            logger.LogWarning(
                exception,
                "Dead-lettering invalid CourseEvent message {MessageId}. Reason: {Reason}.",
                message.MessageId,
                reason);
        }

        await messageActions.DeadLetterMessageAsync(
            message,
            propertiesToModify: null,
            deadLetterReason: reason,
            deadLetterErrorDescription: description,
            cancellationToken: cancellationToken);
    }

    private static async Task CompleteMessageAsync(
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions,
        CancellationToken cancellationToken)
        => await messageActions.CompleteMessageAsync(message, cancellationToken);

    private static void SetActivityTags(
        Activity? activity,
        ServiceBusReceivedMessage message)
    {
        activity?.SetTag("messaging.system", "servicebus");
        activity?.SetTag("messaging.message_id", message.MessageId);
        activity?.SetTag("messaging.correlation_id", message.CorrelationId);
        activity?.SetTag("messaging.delivery_count", message.DeliveryCount);
        activity?.SetTag("messaging.destination", TopicName);
        activity?.SetTag("messaging.destination_kind", "topic");
    }

    private static string? GetTraceProperty(
        ServiceBusReceivedMessage message,
        string propertyName)
        => message.ApplicationProperties.TryGetValue(propertyName, out var value)
            ? value as string
            : null;

    private static bool IsDuplicateOrchestrationException(Exception exception)
        => exception.Message.Contains("already exists", StringComparison.OrdinalIgnoreCase)
           || exception.Message.Contains("already exist", StringComparison.OrdinalIgnoreCase)
           || exception.Message.Contains("duplicate", StringComparison.OrdinalIgnoreCase);
}

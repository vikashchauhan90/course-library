using Azure.Messaging.ServiceBus;
using CourseLibrary.Course.EventConsumer.Observability;
using CourseLibrary.Platform.Messaging.Abstractions;
using CourseLibrary.Platform.Messaging.Observability;
using CourseLibrary.Platform.Serialization.Abstractions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace CourseLibrary.Course.EventConsumer.Consumers.Courses;

internal sealed class CourseEventDqlMessageHandler(
    ISerializerFactory serializerFactory,
    IDqlMessageStore dqlMessageStore,
    ILogger<CourseEventDqlMessageHandler> logger)
{
    private const string DeadLetterDestination = "CourseEvent/$DeadLetterQueue";

    private readonly ISerializer<CourseLibrary.Course.Domain.Events.CourseEvent> _serializer =
        serializerFactory.Create<CourseLibrary.Course.Domain.Events.CourseEvent>(
            SerializerType.Json);

    [Function(nameof(CourseEventDqlMessageHandler))]
    public async Task RunAsync(
        [ServiceBusTrigger(
            DeadLetterDestination,
            "CourseEventConsumer",
            Connection = "ServiceBusConnection",
            AutoCompleteMessages = false)]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions,
        CancellationToken cancellationToken)
    {
        var startedAt = Stopwatch.GetTimestamp();
        var traceParent = ReadStringProperty(message, ServiceBusTraceContext.TraceParent)
            ?? ReadStringProperty(message, ServiceBusTraceContext.DiagnosticId);
        var traceState = ReadStringProperty(message, ServiceBusTraceContext.TraceState);
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

        using var activity = CourseEventConsumerActivitySource.Shared.StartActivity(
            "course.event.deadletter.process",
            ActivityKind.Consumer,
            parentContext);

        try
        {
            activity?.SetTag("messaging.system", "servicebus");
            activity?.SetTag("messaging.destination", DeadLetterDestination);
            activity?.SetTag("messaging.operation.type", "process");
            activity?.SetTag("messaging.message_id", message.MessageId);
            activity?.SetTag("messaging.correlation_id", message.CorrelationId);
            activity?.SetTag("messaging.delivery_count", message.DeliveryCount);
            activity?.SetTag("messaging.dead_letter.reason", message.DeadLetterReason);

            var eventType = "Unknown";
            try
            {
                var courseEvent = _serializer.Deserialize(message.Body.ToArray());
                eventType = courseEvent?.EventType ?? eventType;
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                logger.LogWarning(
                    exception,
                    "Unable to deserialize dead-lettered CourseEvent {MessageId}.",
                    message.MessageId);
            }

            var id = ReadGuidProperty(message, ServiceBusTraceContext.EventId)
                ?? ParseMessageId(message.MessageId)
                ?? CreateStableMessageId(message.MessageId);
            var eventId = ReadGuidProperty(message, ServiceBusTraceContext.EventId) ?? id;
            var record = new DqlMessage
            {
                Id = id,
                EventId = eventId,
                EventType = eventType,
                MessageId = message.MessageId,
                CorrelationId = message.CorrelationId,
                TraceParent = traceParent,
                TraceState = traceState,
                Subject = message.Subject,
                DeadLetterReason = message.DeadLetterReason,
                DeadLetterErrorDescription = message.DeadLetterErrorDescription,
                EnqueuedTime = message.EnqueuedTime,
                DeadLetteredAt = DateTimeOffset.UtcNow,
                DeliveryCount = message.DeliveryCount,
                Payload = Convert.ToBase64String(message.Body.ToArray()),
                Status = DqlMessageStatus.DeadLettered
            };

            await dqlMessageStore.StoreAsync(record, cancellationToken);
            await messageActions.CompleteMessageAsync(message, cancellationToken);

            activity?.SetTag("messaging.event.type", eventType);
            activity?.SetStatus(ActivityStatusCode.Ok);
            CourseEventConsumerMeter.Shared.RecordMessage(
                "dead_letter_stored",
                Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds);
        }
        catch (Exception exception)
        {
            activity?.SetStatus(ActivityStatusCode.Error, exception.GetType().Name);
            CourseEventConsumerMeter.Shared.RecordFailure(
                "deadletter.process",
                exception.GetType().Name);
            logger.LogError(
                exception,
                "Failed to process DQL message {MessageId}.",
                message.MessageId);
            throw;
        }
    }

    private static string? ReadStringProperty(
        ServiceBusReceivedMessage message,
        string name)
        => message.ApplicationProperties.TryGetValue(name, out var value)
            ? value as string
            : null;

    private static Guid? ReadGuidProperty(
        ServiceBusReceivedMessage message,
        string name)
        => Guid.TryParse(ReadStringProperty(message, name), out var id)
            ? id
            : null;

    private static Guid? ParseMessageId(string messageId)
        => Guid.TryParse(messageId, out var id) ? id : null;

    private static Guid CreateStableMessageId(string messageId)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(messageId));
        return new Guid(hash.AsSpan(0, 16));
    }
}

using Azure.Messaging.ServiceBus;
using CourseLibrary.Course.Contracts.DTOs.Courses.Audits;
using CourseLibrary.Course.EventConsumer.Observability;
using CourseLibrary.Platform.Messaging.Abstractions;
using CourseLibrary.Platform.Messaging.Observability;
using CourseLibrary.Platform.Observability.Traces;
using CourseLibrary.Platform.Serialization.Abstractions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace CourseLibrary.Course.EventConsumer.Consumers.Courses;

internal sealed class CourseEventDqlMessageHandler(
    ISerializerFactory serializerFactory,
    IDqlMessageStore dqlMessageStore,
    ILogger<CourseEventDqlMessageHandler> logger)
{
    private const string TopicName = "CourseEvent";
    private const string DeadLetterSubscription = "CourseEventConsumer/$deadletterqueue";

    private readonly ISerializer<CourseEventRequest> _serializer =
        serializerFactory.Create<CourseEventRequest>(SerializerType.Json);

    [Function(nameof(CourseEventDqlMessageHandler))]
    public async Task RunAsync(
        [ServiceBusTrigger(
            TopicName,
            DeadLetterSubscription,
            Connection = "ServiceBusConnection",
            AutoCompleteMessages = true)]
        ServiceBusReceivedMessage message,
        CancellationToken cancellationToken)
    {
        var startedAt = Stopwatch.GetTimestamp();
        var propagationContext = ServiceBusTraceContext.Extract(message);
        var eventMetadata = ServiceBusTraceContext.CreateMetadata(
            message,
            propagationContext);
        var traceParent = eventMetadata.TraceParent;
        var traceState = eventMetadata.TraceState;
        var parentContext = propagationContext.ActivityContext;
        if (parentContext.TraceId == default
            && ActivityContext.TryParse(
                traceParent,
                traceState,
                isRemote: true,
                out var remoteContext))
        {
            parentContext = remoteContext;
        }

        using var activity = CourseEventConsumerActivitySource.Shared.StartActivity(
            "course.event.deadletter.process",
            ActivityKind.Consumer,
            parentContext.TraceId == default ? null : parentContext);

        using var scope = logger.BeginScope(new Dictionary<string, object?>
        {
            [CourseEventConsumerActivityTagConstants.MessagingSystem] = "servicebus",
            [CourseEventConsumerActivityTagConstants.MessagingDestination] = DeadLetterSubscription,
            [CourseEventConsumerActivityTagConstants.MessagingOperation] = "process",
            [CourseEventConsumerActivityTagConstants.MessagingMessageId] = message.MessageId,
            [CourseEventConsumerActivityTagConstants.MessagingCorrelationId] = message.CorrelationId,
            [CourseEventConsumerActivityTagConstants.MessagingDeliveryCount] = message.DeliveryCount
        });

        try
        {
            foreach (var item in eventMetadata.Baggage)
            {
                activity?.AddBaggage(item.Key, item.Value);
            }

            activity?.SetTag(CourseEventConsumerActivityTagConstants.MessagingSystem, "servicebus");
            activity?.SetTag(CourseEventConsumerActivityTagConstants.MessagingDestination, DeadLetterSubscription);
            activity?.SetTag(CourseEventConsumerActivityTagConstants.MessagingOperationType, "process");
            activity?.SetTag(CourseEventConsumerActivityTagConstants.MessagingMessageId, message.MessageId);
            activity?.SetTag(CourseEventConsumerActivityTagConstants.MessagingCorrelationId, message.CorrelationId);
            activity?.SetTag(CourseEventConsumerActivityTagConstants.MessagingDeliveryCount, message.DeliveryCount);
            activity?.SetTag(CourseEventConsumerActivityTagConstants.MessagingDeadLetterReason, message.DeadLetterReason);

            var eventType = "Unknown";
            try
            {
                var courseEvent = _serializer.Deserialize(message.Body.ToArray());
                eventType = courseEvent?.EventType ?? eventType;
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                logger.DeadLetterDeserializationFailed(
                    exception,
                    message.MessageId);
            }

            var id = ReadGuidProperty(
                    eventMetadata.Properties,
                    ServiceBusTraceContext.EventId)
                ?? ParseMessageId(message.MessageId)
                ?? CreateStableMessageId(message.MessageId);
            var eventId = ReadGuidProperty(
                eventMetadata.Properties,
                ServiceBusTraceContext.EventId) ?? id;
            var eventTypeFromMetadata = ReadStringProperty(
                eventMetadata.Properties,
                ServiceBusTraceContext.EventType);
            if (eventType == "Unknown")
            {
                eventType = eventTypeFromMetadata ?? eventType;
            }
            var record = new DqlMessage
            {
                Id = id,
                EventId = eventId,
                EventType = eventType,
                MessageId = message.MessageId,
                CorrelationId = message.CorrelationId,
                TraceParent = traceParent,
                TraceState = traceState,
                Metadata = eventMetadata,
                Subject = message.Subject,
                DeadLetterReason = message.DeadLetterReason,
                DeadLetterErrorDescription = message.DeadLetterErrorDescription,
                EnqueuedTime = message.EnqueuedTime,
                DeadLetteredAt = DateTimeOffset.UtcNow,
                DeliveryCount = message.DeliveryCount,
                Payload = Convert.ToBase64String(message.Body.ToArray()),
                Status = DqlMessageStatus.Pending
            };
            await dqlMessageStore.StoreAsync(record, cancellationToken);
            await dqlMessageStore.StoreAsync(record, cancellationToken);

            activity?.SetTag(CourseEventConsumerActivityTagConstants.MessagingEventType, eventType);
            activity?.SetStatus(ActivityStatusCode.Ok);
            CourseEventConsumerMeter.Shared.RecordMessage(
                "dead_letter_stored",
                Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            activity?.SetTag(CourseEventConsumerActivityTagConstants.MessagingCancelled, true);
            CourseEventConsumerMeter.Shared.RecordFailure("deadletter.process", "cancelled");
            CourseEventConsumerMeter.Shared.RecordMessage(
                "cancelled",
                Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds);
            throw;
        }
        catch (Exception exception)
        {
            ActivityEnrichmentHelper.RecordException(activity, exception);
            CourseEventConsumerMeter.Shared.RecordFailure(
                "deadletter.process",
                exception.GetType().Name);
            CourseEventConsumerMeter.Shared.RecordMessage(
                "failure",
                Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds);
            logger.DeadLetterProcessingFailed(
                exception,
                message.MessageId);
            throw;
        }
    }

    private static string? ReadStringProperty(
        ImmutableDictionary<string, string> properties,
        string name)
        => properties.TryGetValue(name, out var value) ? value : null;

    private static Guid? ReadGuidProperty(
        ImmutableDictionary<string, string> properties,
        string name)
        => Guid.TryParse(ReadStringProperty(properties, name), out var id)
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

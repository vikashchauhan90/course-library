using Azure.Messaging.ServiceBus;
using CourseLibrary.Application.Abstractions.Messaging;
using CourseLibrary.Application.Abstractions.Serialization;
using CourseLibrary.Application.Abstractions.Serializers;
using CourseLibrary.Domain.Events;
using CourseLibrary.Domain.ValueObjects;
using CourseLibrary.EventConsumer.Configuration.Observability.Metrics;
using CourseLibrary.EventConsumer.Configuration.Observability.Traces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using InfraTraces = CourseLibrary.Infrastructure.Observability.Traces;


namespace CourseLibrary.EventConsumer.Consumers.Courses;

internal sealed class CourseEventDqlMessageHandler(
    ISerializerFactory serializerFactory,
    IDqlMessageStore dqlMessageStore,
    ILogger<CourseEventDqlMessageHandler> logger)
{
    private readonly ISerializer<CourseEvent> serializer =
        serializerFactory.Create<CourseEvent>(
            SerializerType.Json);

    [Function(nameof(CourseEventDqlMessageHandler))]
    public async Task RunAsync(
        [ServiceBusTrigger(
            "CourseEvent/$DeadLetterQueue",
            "CourseEventConsumer",
            Connection = "ServiceBusConnection",
            AutoCompleteMessages = false)]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions,
        CancellationToken cancellationToken)
    {
        var started = Stopwatch.GetTimestamp();
        var eventType = "Unknown";
        var propagationContext =
            InfraTraces.ServiceBusTraceContext.Extract(message);

        using var scope = logger.BeginScope(
            new Dictionary<string, object?>
            {
                ["messaging.system"] = "servicebus",
                ["messaging.destination"] = "CourseEvent/$DeadLetterQueue",
                ["messaging.operation"] = "deadletter.process",
                ["messaging.message_id"] = message.MessageId,
                ["messaging.correlation_id"] = message.CorrelationId,
                ["messaging.delivery_count"] = message.DeliveryCount,
                ["messaging.dead_letter_reason"] = message.DeadLetterReason
            });

        using var activity = ActivitySources.StartActivity(
            "course.event.deadletter.process",
            ActivityKind.Consumer,
            propagationContext.ActivityContext);

        try
        {
            activity?.SetTag("messaging.system", "servicebus");
            activity?.SetTag("messaging.destination", "CourseEvent/$DeadLetterQueue");
            activity?.SetTag("messaging.operation.type", "process");
            activity?.SetTag("messaging.message_id", message.MessageId);
            activity?.SetTag("messaging.correlation_id", message.CorrelationId);
            activity?.SetTag("messaging.delivery_count", message.DeliveryCount);
            activity?.SetTag("messaging.dead_letter.reason", message.DeadLetterReason);

            CourseEvent? courseEvent = null;

            try
            {
                courseEvent = serializer.Deserialize(
                    message.Body.ToArray());
            }
            catch (Exception exception)
            {
                logger.LogWarning(
                    exception,
                    "Unable to deserialize dead-lettered CourseEvent {MessageId}.",
                    message.MessageId);
            }

            eventType = courseEvent?.EventType.ToString() ?? "Unknown";

            var record = new DqlMessage
            {
                Id = message.MessageId,
                EventType = eventType,

                MessageId = message.MessageId,
                CorrelationId = message.CorrelationId,
                TraceParent = InfraTraces.ServiceBusTraceContext.GetTraceParent(message),
                TraceState = InfraTraces.ServiceBusTraceContext.GetTraceState(message),
                Subject = message.Subject,

                DeadLetterReason =
                    message.DeadLetterReason,

                DeadLetterErrorDescription =
                    message.DeadLetterErrorDescription,

                EnqueuedTime = message.EnqueuedTime,
                DeadLetteredAt = DateTimeOffset.UtcNow,

                DeliveryCount = message.DeliveryCount,

                Payload = Convert.ToBase64String(
                    message.Body.ToArray()),

                Status = DqlMessageStatus.DeadLettered
            };

            await dqlMessageStore.StoreAsync(
                record,
                cancellationToken);

            await messageActions.CompleteMessageAsync(
                message,
                cancellationToken);

            activity?.SetTag("messaging.event_type", eventType);
            activity?.SetStatus(ActivityStatusCode.Ok);
            Meters.RecordMessageProcessed(
                "CourseEvent",
                message.MessageId,
                "CourseEvent/$DeadLetterQueue",
                Stopwatch.GetElapsedTime(started).TotalMilliseconds);
        }
        catch (Exception exception)
        {
            activity?.SetStatus(ActivityStatusCode.Error, exception.GetType().Name);
            Meters.RecordMessageFailed(
                "CourseEvent",
                message.MessageId,
                "CourseEvent/$DeadLetterQueue",
                exception);
            logger.LogError(
                exception,
                "Failed to process DQL message {MessageId}.",
                message.MessageId);

            throw;
        }
    }
}
using Azure.Messaging.ServiceBus;
using CourseLibrary.Messaging.Abstractions;
using CourseLibrary.Messaging.Observability;
using CourseLibrary.Platform.Observability.Traces;
using CourseLibrary.Platform.Serialization.Abstractions;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CourseLibrary.Messaging;

internal sealed class ServiceBusEventPublisher(
    ServiceBusClient client,
    ILogger<ServiceBusEventPublisher> logger)
    : IEventPublisher
{
    private static readonly PlatformActivitySource ActivitySources = new();
    public async Task PublishAsync(
        string queueOrTopicName,
      ServiceBusEventPayload @event,
      CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(@event);
        using var activity = ActivitySources.StartActivity(
            "ServiceBusEventPublisher.PublishAsync",
            ActivityKind.Producer);

        try
        {
            logger.LogInformation(
                "Current trace context: TraceId={TraceId}, SpanId={SpanId}, ParentSpanId={ParentSpanId}",
                Activity.Current?.TraceId.ToString(),
                Activity.Current?.SpanId.ToString(),
                Activity.Current?.ParentSpanId.ToString());

            var propagationActivity = activity ?? Activity.Current;

            var userId = @event.Metadata.Properties.TryGetValue("userId", out var userIdValue) ? userIdValue : null;
            var correlationId = @event.Metadata.Properties.TryGetValue("correlationId", out var correlationIdValue) ? correlationIdValue : null;
            var tenantId = @event.Metadata.Properties.TryGetValue("tenantId", out var tenantIdValue) ? tenantIdValue : null;

            // Set tags
            activity?.SetTag("event.type", @event.EventType);
            activity?.SetTag("event.id", @event.EventId);
            activity?.SetTag("event.occurredAt", @event.OccurredAt.ToUnixTimeMilliseconds());
            activity?.SetTag("request.correlationId", correlationId);
            activity?.SetTag("request.userId", userId);
            activity?.SetTag("request.traceId", propagationActivity?.TraceId.ToString());
            activity?.SetTag("message.contentType", SerializerType.Json.ToString());
            activity?.SetTag("message.size", @event.Payload.Length);
            activity?.SetTag("message.id", @event.EventId.ToString());
            activity?.SetTag("message.subject", @event.EventType);

            var message = new ServiceBusMessage(@event.Payload)
            {
                MessageId = @event.EventId.ToString(),
                Subject = @event.EventType,
                ContentType = SerializerType.Json.ToString(),
                CorrelationId = correlationId,
                ApplicationProperties =
            {
                [ServiceBusTraceContext.EventId] = @event.EventId.ToString(),
                [ServiceBusTraceContext.EventOccurredAt] = @event.OccurredAt.ToUnixTimeMilliseconds(),
                [ServiceBusTraceContext.EventType] = @event.EventType,
                [ServiceBusTraceContext.Destination] = queueOrTopicName,
                [ServiceBusTraceContext.UserId] = userId,
                [ServiceBusTraceContext.Source] = nameof(ServiceBusEventPublisher),
                [ServiceBusTraceContext.TenantId] = tenantId,
            }
            };

            // Inject trace context
            if (propagationActivity is not null)
            {
                ServiceBusTraceContext.Inject(message, propagationActivity);
            }

            // Check message size limits
            if (@event.Payload.Length > 256 * 1024) // 256 KB for Standard tier
            {
                logger.LogWarning(
                    "Message {EventId} size {Size} bytes exceeds recommended limit for Service Bus Standard tier.",
                    @event.EventId, @event.Payload.Length);
            }

            await using var sender = client.CreateSender(queueOrTopicName);

            var sendStartTime = Stopwatch.GetTimestamp();

            // Record message size metrics

            await sender.SendMessageAsync(message, cancellationToken);

            // Set success tags
            activity?.SetTag("message.status", "sent");
            activity?.SetTag("message.sentAt", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            activity?.SetTag("message.sentBy", nameof(ServiceBusEventPublisher));
            activity?.SetTag("message.sentTo", queueOrTopicName);
            activity?.SetTag("message.sentByService", nameof(ServiceBusClient));
            activity?.SetTag("message.sentByServiceVersion",
                client.GetType().Assembly.GetName().Version?.ToString() ?? "unknown");
            activity?.SetTag("message.duration_ms",
                (Stopwatch.GetElapsedTime(sendStartTime).TotalMilliseconds));

            activity?.SetStatus(ActivityStatusCode.Ok);

            logger.LogInformation(
                "Published integration event {EventType} with EventId {EventId} to {Destination} in {Duration}ms.",
                @event.EventType, @event.EventId, queueOrTopicName,
                (Stopwatch.GetElapsedTime(sendStartTime).TotalMilliseconds));
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.SetTag("error.type", ex.GetType().Name);
            activity?.SetTag("error.message", ex.Message);

            logger.LogError(ex,
                "Failed to publish event {EventType} with EventId {EventId}.",
                @event.EventType, @event.EventId);

            throw;
        }
    }
}
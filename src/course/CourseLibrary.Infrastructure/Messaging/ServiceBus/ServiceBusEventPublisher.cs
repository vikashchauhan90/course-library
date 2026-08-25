using Azure.Messaging.ServiceBus;
using CourseLibrary.Application.Abstractions.Messaging;
using CourseLibrary.Application.Abstractions.RequestContext;
using CourseLibrary.Application.Abstractions.Serialization;
using CourseLibrary.Application.Abstractions.Serializers;
using CourseLibrary.Domain.Events;
using CourseLibrary.Infrastructure.Observability.Metrics;
using CourseLibrary.Infrastructure.Observability.Traces;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CourseLibrary.Infrastructure.Messaging.ServiceBus;

internal sealed class ServiceBusEventPublisher(
    ServiceBusClient client,
    IRequestContext requestContext,
    IEventRouter router,
    ISerializerFactory serializerFactory,
    ILogger<ServiceBusEventPublisher> logger)
    : IEventPublisher
{
    private readonly ISerializer<object> _serializer =
        serializerFactory.Create<object>(
            SerializerType.MessagePack);


    public async Task PublishAsync<TEvent>(
      TEvent @event,
      CancellationToken cancellationToken = default)
      where TEvent : IDomainEvent
    {
        using var activity = ActivitySources.Infrastructure.StartActivity(
            "ServiceBusEventPublisher.PublishAsync",
            ActivityKind.Producer);

        try
        {
            var propagationActivity = activity ?? Activity.Current;
            var eventType = typeof(TEvent).Name;
            var destination = router.GetDestination<TEvent>();
            var messageChannelType = router.GetChannelType<TEvent>();
            var serialized = _serializer.Serialize(@event);

            // Set tags
            activity?.SetTag("event.type", eventType);
            activity?.SetTag("event.id", @event.EventId);
            activity?.SetTag("event.occurredAt", @event.OccurredAt.ToUnixTimeMilliseconds());
            activity?.SetTag("message.channelType", messageChannelType.ToString());
            activity?.SetTag("message.destination", destination);
            activity?.SetTag("request.correlationId", requestContext.CorrelationId);
            activity?.SetTag("request.userId", requestContext.UserId);
            activity?.SetTag("request.traceId", propagationActivity?.TraceId.ToString());
            activity?.SetTag("message.contentType", SerializerType.MessagePack.ToString());
            activity?.SetTag("message.size", serialized.Length);
            activity?.SetTag("message.id", @event.EventId.ToString());
            activity?.SetTag("message.subject", eventType);

            var message = new ServiceBusMessage(BinaryData.FromBytes(serialized))
            {
                MessageId = @event.EventId.ToString(),
                Subject = eventType,
                ContentType = SerializerType.MessagePack.ToString(),
                CorrelationId = requestContext.CorrelationId,
                ApplicationProperties =
            {
                ["EventId"] = @event.EventId.ToString(),
                ["OccurredAt"] = @event.OccurredAt.ToUnixTimeMilliseconds(),
                ["EventType"] = eventType,
                ["MessageChannelType"] = messageChannelType.ToString(),
                ["Destination"] = destination,
                ["UserId"] = requestContext.UserId
            }
            };

            // Inject trace context
            if (propagationActivity is not null)
            {
                ServiceBusTraceContext.Inject(message, propagationActivity);
            }

            // Check message size limits
            if (serialized.Length > 256 * 1024) // 256 KB for Standard tier
            {
                logger.LogWarning(
                    "Message {EventId} size {Size} bytes exceeds recommended limit for Service Bus Standard tier.",
                    @event.EventId, serialized.Length);
            }

            await using var sender = client.CreateSender(destination);

            var sendStartTime = Stopwatch.GetTimestamp();

            // Record message size metrics

            await sender.SendMessageAsync(message, cancellationToken);

            // Set success tags
            activity?.SetTag("message.status", "sent");
            activity?.SetTag("message.sentAt", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            activity?.SetTag("message.sentBy", nameof(ServiceBusEventPublisher));
            activity?.SetTag("message.sentTo", destination);
            activity?.SetTag("message.sentByService", nameof(ServiceBusClient));
            activity?.SetTag("message.sentByServiceVersion",
                client.GetType().Assembly.GetName().Version?.ToString() ?? "unknown");
            activity?.SetTag("message.duration_ms",
                (Stopwatch.GetElapsedTime(sendStartTime).TotalMilliseconds));

            activity?.SetStatus(ActivityStatusCode.Ok);

            logger.LogInformation(
                "Published integration event {EventType} with EventId {EventId} to {Destination} in {Duration}ms.",
                eventType, @event.EventId, destination,
                (Stopwatch.GetElapsedTime(sendStartTime).TotalMilliseconds));
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.SetTag("error.type", ex.GetType().Name);
            activity?.SetTag("error.message", ex.Message);

            logger.LogError(ex,
                "Failed to publish event {EventType} with EventId {EventId}.",
                typeof(TEvent).Name, @event.EventId);

            throw;
        }
    }
}
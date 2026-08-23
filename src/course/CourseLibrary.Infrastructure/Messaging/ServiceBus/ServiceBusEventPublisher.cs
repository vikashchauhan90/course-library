using Azure.Messaging.ServiceBus;
using CourseLibrary.Application.Abstractions.Messaging;
using CourseLibrary.Application.Abstractions.RequestContext;
using CourseLibrary.Application.Abstractions.Serialization;
using CourseLibrary.Application.Abstractions.Serializers;
using CourseLibrary.Domain.Events;
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

        var eventType = typeof(TEvent).Name;
        var destination = router.GetDestination<TEvent>();
        var messageChannelType = router.GetChannelType<TEvent>();
        var serialized = _serializer.Serialize(@event);

        activity?.SetTag("event.type", eventType);
        activity?.SetTag("event.id", @event.EventId);
        activity?.SetTag("event.occurredAt", @event.OccurredAt.ToUnixTimeMilliseconds());
        activity?.SetTag("message.channelType", messageChannelType.ToString());
        activity?.SetTag("message.destination", destination);
        activity?.SetTag("request.correlationId", requestContext.CorrelationId);
        activity?.SetTag("request.userId", requestContext.UserId);
        activity?.SetTag("request.traceParent", requestContext.TraceParent);
        activity?.SetTag("request.traceId", requestContext.TraceId);
        activity?.SetTag("request.traceState", requestContext.TraceState);
        activity?.SetTag("message.contentType", SerializerType.MessagePack.ToString());
        activity?.SetTag("message.size", serialized.Length);
        activity?.SetTag("message.id", @event.EventId.ToString());
        activity?.SetTag("message.subject", eventType);

        var message = new ServiceBusMessage(
            BinaryData.FromBytes(serialized))
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
                ["UserId"] = requestContext.UserId,
                ["TraceParent"] = requestContext.TraceParent,
                ["TraceId"] = requestContext.TraceId,
                ["TraceState"] = requestContext.TraceState
            }

        };

        logger.LogInformation(
            "Publishing integration event {EventType} with EventId {EventId} to {MessageChannelType} {TopicName}.",
            eventType,
            @event.EventId,
            messageChannelType.ToString(),
            destination);

        await using var sender =
            client.CreateSender(destination);

        await sender.SendMessageAsync(
            message,
            cancellationToken);

        activity?.SetTag("message.status", "sent");
        activity?.SetTag("message.sentAt", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        activity?.SetTag("message.sentBy", nameof(ServiceBusEventPublisher));
        activity?.SetTag("message.sentTo", destination);
        activity?.SetTag("message.sentByService", nameof(ServiceBusClient));
        activity?.SetTag("message.sentByServiceVersion", client.GetType().Assembly.GetName().Version?.ToString() ?? "unknown");

        logger.LogInformation(
            "Published integration event {EventType} with EventId {EventId}.",
            eventType,
            @event.EventId);
    }
}
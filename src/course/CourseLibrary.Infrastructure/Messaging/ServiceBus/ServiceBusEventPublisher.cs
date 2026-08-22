using Azure.Messaging.ServiceBus;
using CourseLibrary.Application.Abstractions.Messaging;
using CourseLibrary.Application.Abstractions.RequestContext;
using CourseLibrary.Application.Abstractions.Serialization;
using CourseLibrary.Application.Abstractions.Serializers;
using CourseLibrary.Domain.Events;
using Microsoft.Extensions.Logging;

namespace CourseLibrary.Infrastructure.Messaging.ServiceBus;

internal sealed class ServiceBusEventPublisher(
    ServiceBusClient client,
    IRequestContext requestContext,
    IEventRouter router,
    ISerializerFactory serializerFactory,
    ILogger<ServiceBusEventPublisher> logger)
    : IEventPublisher
{
    private readonly ISerializer<IDomainEvent> _serializer =
        serializerFactory.Create<IDomainEvent>(
            SerializerType.MessagePack);


    public async Task PublishAsync<TEvent>(
        TEvent @event,
        CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent
    {
        var eventType = typeof(TEvent).Name;
        var destination = router.GetDestination<TEvent>();
        var messageChannelType = router.GetChannelType<TEvent>();
        var serialized = _serializer.Serialize(@event);

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

        logger.LogInformation(
            "Published integration event {EventType} with EventId {EventId}.",
            eventType,
            @event.EventId);
    }
}
using Azure.Messaging.ServiceBus;
using CourseLibrary.Application.Abstractions.Messaging;
using CourseLibrary.Application.Abstractions.RequestContext;
using CourseLibrary.Domain.Events;
using CourseLibrary.Infrastructure.Configuration.Messaging;
using CourseLibrary.Infrastructure.Observability.Traces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CourseLibrary.Infrastructure.Messaging;

internal sealed class ServiceBusEventPublisher(
    ServiceBusClient client,
    IOptions<ServiceBusOptions> options,
    IRequestContext requestContext,
    ILogger<ServiceBusEventPublisher> logger)
    : IEventPublisher
{
    public async Task PublishAsync<TEvent>(
        TEvent @event,
        CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent
    {
        var eventType = typeof(TEvent).Name;
        var topicName = GetTopicName<TEvent>();

        var message = new ServiceBusMessage(
            BinaryData.FromObjectAsJson(@event));

        message.MessageId =
            @event.EventId.ToString();

        message.Subject = eventType;

        message.ApplicationProperties["EventId"] =
            @event.EventId.ToString();

        message.ApplicationProperties["OccurredAt"] =
            @event.OccurredAt.ToUnixTimeMilliseconds();

        if (!string.IsNullOrWhiteSpace(requestContext.TraceId))
        {
            message.ApplicationProperties[TraceHeaders.TraceId] =
                requestContext.TraceId;
        }

        if (!string.IsNullOrWhiteSpace(requestContext.CorrelationId))
        {
            message.ApplicationProperties[TraceHeaders.CorrelationId] =
                requestContext.CorrelationId;
        }

        if (!string.IsNullOrWhiteSpace(requestContext.TraceParent))
        {
            message.ApplicationProperties[TraceHeaders.TraceParent] =
                requestContext.TraceParent;
        }

        logger.LogInformation(
            "Publishing integration event {EventType} with EventId {EventId} to topic {TopicName}.",
            eventType,
            @event.EventId,
            topicName);

        await using var sender =
            client.CreateSender(topicName);

        await sender.SendMessageAsync(
            message,
            cancellationToken);

        logger.LogInformation(
            "Published integration event {EventType} with EventId {EventId}.",
            eventType,
            @event.EventId);
    }

    private string GetTopicName<TEvent>()
        where TEvent : IDomainEvent
    {
        return typeof(TEvent).Name switch
        {
            nameof(AuthorCreatedIntegrationEvent)
                => options.Value.Topics.AuthorEvents,

            nameof(AuthorAuditIntegrationEvent)
                => options.Value.Topics.AuditEvents,

            nameof(AuthorNotificationIntegrationEvent)
                => options.Value.Topics.NotificationEvents,

            _ => throw new InvalidOperationException(
                $"No Service Bus topic configured for event '{typeof(TEvent).Name}'.")
        };
    }
}
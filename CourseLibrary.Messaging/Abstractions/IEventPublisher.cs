namespace CourseLibrary.Messaging.Abstractions;

public interface IEventPublisher
{
    Task PublishAsync(
        string queueOrTopicName,
        ServiceBusEventPayload @event,
        CancellationToken cancellationToken = default);
}
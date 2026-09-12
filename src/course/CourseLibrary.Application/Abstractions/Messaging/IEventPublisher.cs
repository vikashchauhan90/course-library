using CourseLibrary.Domain.Abstractions;

namespace CourseLibrary.Application.Abstractions.Messaging;

public interface IEventPublisher
{
    Task PublishAsync<TEvent>(
        string queueOrTopicName,
        TEvent @event,
        CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent;
}
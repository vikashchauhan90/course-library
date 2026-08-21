using CourseLibrary.Domain.Events;

namespace CourseLibrary.Application.Abstractions.Messaging;

public interface IEventPublisher
{
    Task PublishAsync<TEvent>(
        TEvent @event,
        CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent;
}
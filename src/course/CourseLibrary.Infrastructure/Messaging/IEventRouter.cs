using CourseLibrary.Domain.Events;

namespace CourseLibrary.Infrastructure.Messaging;

public interface IEventRouter
{
    string GetDestination<TEvent>() where TEvent : IDomainEvent;
    MessageChannelType GetChannelType<TEvent>() where TEvent : IDomainEvent;
}

using CourseLibrary.Domain.Events;
using System.Collections.Concurrent;
using System.Reflection;

namespace CourseLibrary.Infrastructure.Messaging;


internal sealed class EventRouter : IEventRouter
{
    private static readonly ConcurrentDictionary<Type, (string Destination, MessageChannelType Type)> Cache = new();

    public string GetDestination<TEvent>() where TEvent : IDomainEvent
    {
        var (destination, _) = GetRoutingInfo<TEvent>();
        return destination;
    }

    public MessageChannelType GetChannelType<TEvent>() where TEvent : IDomainEvent
    {
        var (_, type) = GetRoutingInfo<TEvent>();
        return type;
    }

    private static (string Destination, MessageChannelType Type) GetRoutingInfo<TEvent>()
    {
        var eventType = typeof(TEvent);

        return Cache.GetOrAdd(eventType, type =>
        {
            var attribute = type.GetCustomAttribute<EventRoutingAttribute>();

            if (attribute is null)
            {
                throw new InvalidOperationException(
                    $"No routing configured for event '{type.Name}'. " +
                    $"Add [EventRouting] attribute to the event class.");
            }

            return (attribute.Destination, attribute.Type);
        });
    }
}
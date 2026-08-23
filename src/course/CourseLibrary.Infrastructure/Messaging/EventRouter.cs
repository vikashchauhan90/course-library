using CourseLibrary.Domain.Events;
using CourseLibrary.Infrastructure.Observability.Traces;
using System.Collections.Concurrent;
using System.Diagnostics;
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
        using var activity = ActivitySources.Infrastructure.StartActivity("GetRoutingInfo", ActivityKind.Internal);
        var eventType = typeof(TEvent);

        activity?.SetTag("eventType", eventType.FullName);

        return Cache.GetOrAdd(eventType, type =>
        {
            var attribute = type.GetCustomAttribute<EventRoutingAttribute>();

            if (attribute is null)
            {
                activity?.SetStatus(ActivityStatusCode.Error, "No routing configured for event.");
                throw new InvalidOperationException(
                    $"No routing configured for event '{type.Name}'. " +
                    $"Add [EventRouting] attribute to the event class.");
            }

            activity?.SetTag("destination", attribute.Destination);
            activity?.SetTag("channelType", attribute.Type.ToString());

            return (attribute.Destination, attribute.Type);
        });
    }
}
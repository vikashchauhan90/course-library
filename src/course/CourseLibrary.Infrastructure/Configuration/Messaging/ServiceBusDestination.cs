using CourseLibrary.Domain.Events;

namespace CourseLibrary.Infrastructure.Configuration.Messaging;

internal sealed record ServiceBusDestination(
    ServiceBusDestinationType Type,
    string Name)
{

    private static readonly IReadOnlyDictionary<Type, ServiceBusDestination> Destinations =
    new Dictionary<Type, ServiceBusDestination>
    {
        [typeof(AuthorCreatedEvent)] =
            new(ServiceBusDestinationType.Topic, "AuthorEvents"),
    };

    public static ServiceBusDestination GetDestination<TEvent>()
    where TEvent : IDomainEvent
    {
        if (!ServiceBusDestination.Destinations.TryGetValue(
                typeof(TEvent),
                out var destination))
        {
            throw new InvalidOperationException(
                $"No Service Bus destination configured for event '{typeof(TEvent).Name}'.");
        }

        return destination;
    }
}


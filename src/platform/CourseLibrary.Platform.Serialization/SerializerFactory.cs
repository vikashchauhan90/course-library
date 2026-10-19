using CourseLibrary.Platform.Serialization.Abstractions;
using CourseLibrary.Platform.Serialization.Observability;
using Microsoft.Extensions.DependencyInjection;

namespace CourseLibrary.Platform.Serialization;

internal sealed class SerializerFactory(
    IServiceProvider serviceProvider) : ISerializerFactory
{
    private static readonly SerializationActivitySource ActivitySources = SerializationActivitySource.Shared;
    public ISerializer<T> Create<T>(SerializerType type)
    {
        using var activity = ActivitySources.StartActivity(
            $"{nameof(SerializerFactory)}.{nameof(Create)}");

        activity?.AddTag("serializer.type", type.ToString());

        var key = type switch
        {
            SerializerType.Json =>
            nameof(SerializerType.Json),
            SerializerType.MessagePack =>
            nameof(SerializerType.MessagePack),
            _ => throw new ArgumentOutOfRangeException(
                nameof(type),
                type,
                "Unsupported serializer type.")
        };
        activity?.AddTag("serializer.key", key);
        return serviceProvider.GetRequiredKeyedService<ISerializer<T>>(key);
    }
}
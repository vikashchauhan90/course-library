using CourseLibrary.Application.Abstractions.Serialization;
using CourseLibrary.Application.Abstractions.Serializers;
using CourseLibrary.Infrastructure.Observability.Traces;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace CourseLibrary.Infrastructure.Serializers;

public sealed class SerializerFactory(
    IServiceProvider serviceProvider) : ISerializerFactory
{
    public ISerializer<T> Create<T>(SerializerType type)
    {
        using var activity = ActivitySources.Infrastructure.StartActivity(
            $"{nameof(SerializerFactory)}.{nameof(Create)}",
            ActivityKind.Internal);

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
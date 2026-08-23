using CourseLibrary.Idp.Application.Abstractions.Serialization;
using CourseLibrary.Idp.Application.Abstractions.Serializers;
using CourseLibrary.Idp.Infrastructure.Observability.Traces;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace CourseLibrary.Idp.Infrastructure.Serializers;

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
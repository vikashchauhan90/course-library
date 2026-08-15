using CourseLibrary.Application.Abstractions.Serialization;
using CourseLibrary.Application.Abstractions.Serializers;
using Microsoft.Extensions.DependencyInjection;

namespace CourseLibrary.Infrastructure.Serializers;

public sealed class SerializerFactory(
    IServiceProvider serviceProvider) : ISerializerFactory
{
    public ISerializer<T> Create<T>(SerializerType type)
    {
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

        return serviceProvider.GetRequiredKeyedService<ISerializer<T>>(key);
    }
}
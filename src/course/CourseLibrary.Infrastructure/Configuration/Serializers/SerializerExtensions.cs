using CourseLibrary.Application.Abstractions.Serialization;
using CourseLibrary.Application.Abstractions.Serializers;
using CourseLibrary.Infrastructure.Configuration.Cosmos.Serialization;
using CourseLibrary.Infrastructure.Serializers;
using Microsoft.Extensions.DependencyInjection;

namespace CourseLibrary.Infrastructure.Configuration.Serializers;

public static class SerializerExtensions
{
    public static IServiceCollection AddCourseLibrarySerializers(
    this IServiceCollection services)
    {
        var options = ValueObjectJsonConverters.CreateOptions();

        services.AddSingleton(options);

        services.AddKeyedSingleton(
            typeof(ISerializer<>),
            nameof(SerializerType.Json),
            typeof(SystemTextJsonSerializer<>));

        services.AddKeyedSingleton(
            typeof(ISerializer<>),
            nameof(SerializerType.MessagePack),
            typeof(MessagePackSerializer<>));

        services.AddSingleton<ISerializerFactory, SerializerFactory>();

        return services;
    }
}

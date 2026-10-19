using System.Text.Json;
using CourseLibrary.Platform.Serialization.Abstractions;
using CourseLibrary.Platform.Serialization.Serializers;
using Microsoft.Extensions.DependencyInjection;

namespace CourseLibrary.Platform.Serialization;

public static class SerializationServiceCollectionExtensions
{
    public static IServiceCollection AddCourseLibrarySerializers(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton(new JsonSerializerOptions(JsonSerializerDefaults.Web));
        services.AddSingleton<ISerializerFactory, SerializerFactory>();
        services.AddKeyedTransient(
            typeof(ISerializer<>),
            nameof(SerializerType.Json),
            typeof(SystemTextJsonSerializer<>));
        services.AddKeyedTransient(
            typeof(ISerializer<>),
            nameof(SerializerType.MessagePack),
            typeof(MessagePackSerializer<>));

        return services;
    }
}

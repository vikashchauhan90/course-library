using CourseLibrary.Application.Abstractions.Serialization;
using CourseLibrary.Application.Abstractions.Serializers;
using CourseLibrary.Infrastructure.Serializers;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace CourseLibrary.Infrastructure.Configuration.Serializers;

public static class SerializerExtensions
{
    public static IServiceCollection AddCourseLibrarySerializers(
    this IServiceCollection services)
    {
        services.AddSingleton(
        new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

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

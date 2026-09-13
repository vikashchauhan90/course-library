using CourseLibrary.Application.Abstractions.Serialization;
using CourseLibrary.Application.Abstractions.Serializers;
using CourseLibrary.Infrastructure.Configuration.Cosmos.Serialization;
using CourseLibrary.Infrastructure.Serializers;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace CourseLibrary.Infrastructure.Configuration.Serializers;

public static class SerializerExtensions
{
    public static IServiceCollection AddCourseLibrarySerializers(
    this IServiceCollection services)
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        options.Converters.Add(new AuthorIdJsonConverter());
        options.Converters.Add(new AuditActionJsonConverter());
        options.Converters.Add(new CodeJsonConverter());
        options.Converters.Add(new CommentIdJsonConverter());
        options.Converters.Add(new CourseEventTypeJsonConverter());
        options.Converters.Add(new CourseIdJsonConverter());
        options.Converters.Add(new DiscussionIdJsonConverter());
        options.Converters.Add(new DqlMessageStatusJsonConverter());
        options.Converters.Add(new MoneyJsonConverter());

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

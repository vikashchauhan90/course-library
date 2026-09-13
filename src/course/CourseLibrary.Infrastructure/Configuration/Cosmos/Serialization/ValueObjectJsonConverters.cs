using System.Text.Json;

namespace CourseLibrary.Infrastructure.Configuration.Cosmos.Serialization;

internal static class ValueObjectJsonConverters
{
    public static JsonSerializerOptions CreateOptions()
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

        return options;
    }
}
using CourseLibrary.Course.Domain.ValueObjects;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Comment = CourseLibrary.Course.Domain.Entities.Comment;
using CourseAuditEntry = CourseLibrary.Course.Domain.Entities.CourseAuditEntry;
using Discussion = CourseLibrary.Course.Domain.Entities.Discussion;

namespace CourseLibrary.Course.Persistence.Data.Configurations.Serialization;

internal static class ValueObjectJsonConverters
{
    public static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            TypeInfoResolver = CreateTypeInfoResolver()
        };

        options.Converters.Add(new AuthorIdJsonConverter());
        options.Converters.Add(new CommentIdJsonConverter());
        options.Converters.Add(new CourseEventTypeJsonConverter());
        options.Converters.Add(new CourseIdJsonConverter());
        options.Converters.Add(new DiscussionIdJsonConverter());

        return options;
    }

    private static IJsonTypeInfoResolver CreateTypeInfoResolver()
    {
        var resolver = new DefaultJsonTypeInfoResolver();
        resolver.Modifiers.Add(typeInfo =>
        {
            if (typeInfo.Kind != JsonTypeInfoKind.Object)
            {
                return;
            }

            var documentType = typeInfo.Type;
            foreach (var property in typeInfo.Properties)
            {
                if (documentType == typeof(CourseLibrary.Course.Domain.Entities.Course) &&
                    property.Name.Equals(
                        "authorId",
                        StringComparison.OrdinalIgnoreCase))
                {
                    property.CustomConverter = new AuthorIdPartitionKeyJsonConverter();
                }
                else if (property.Name.Equals(
                        "courseId",
                        StringComparison.OrdinalIgnoreCase) &&
                    (documentType == typeof(Comment) ||
                     documentType == typeof(Discussion) ||
                     documentType == typeof(CourseAuditEntry)))
                {
                    property.CustomConverter = new CourseIdPartitionKeyJsonConverter();
                }
            }
        });

        return resolver;
    }

    private sealed class AuthorIdPartitionKeyJsonConverter : JsonConverter<AuthorId>
    {
        public override AuthorId Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            var value = reader.GetString() ?? string.Empty;
            if (value.StartsWith(
                CoursePartitionKeyFormats.AuthorPrefix,
                StringComparison.Ordinal))
            {
                value = value[CoursePartitionKeyFormats.AuthorPrefix.Length..];
            }

            return new AuthorId(value);
        }

        public override void Write(
            Utf8JsonWriter writer,
            AuthorId value,
            JsonSerializerOptions options)
        {
            writer.WriteStringValue(
                CoursePartitionKeyFormats.AuthorPrefix + value.Value);
        }
    }

    private sealed class CourseIdPartitionKeyJsonConverter : JsonConverter<CourseId>
    {
        public override CourseId Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            var value = reader.GetString() ?? string.Empty;
            if (value.StartsWith(
                CoursePartitionKeyFormats.CoursePrefix,
                StringComparison.Ordinal))
            {
                value = value[CoursePartitionKeyFormats.CoursePrefix.Length..];
            }

            return new CourseId(value);
        }

        public override void Write(
            Utf8JsonWriter writer,
            CourseId value,
            JsonSerializerOptions options)
        {
            writer.WriteStringValue(
                CoursePartitionKeyFormats.CoursePrefix + value.Value);
        }
    }
}
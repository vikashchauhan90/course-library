using CourseLibrary.Domain.ValueObjects;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CourseLibrary.Infrastructure.Configuration.Cosmos.Serialization;

public sealed class DiscussionIdJsonConverter : JsonConverter<DiscussionId>
{
    public override DiscussionId Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        return new DiscussionId(reader.GetGuid());
    }

    public override void Write(
        Utf8JsonWriter writer,
        DiscussionId value,
        JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}
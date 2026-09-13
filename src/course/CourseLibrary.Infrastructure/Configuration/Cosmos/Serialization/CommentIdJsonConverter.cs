using CourseLibrary.Domain.ValueObjects;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CourseLibrary.Infrastructure.Configuration.Cosmos.Serialization;

public sealed class CommentIdJsonConverter : JsonConverter<CommentId>
{
    public override CommentId Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        return new CommentId(reader.GetGuid());
    }

    public override void Write(
        Utf8JsonWriter writer,
        CommentId value,
        JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}
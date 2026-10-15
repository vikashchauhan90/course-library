using CourseLibrary.Course.Domain.ValueObjects;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CourseLibrary.Course.Persistence.Data.Serialization;

internal sealed class AuthorIdJsonConverter : JsonConverter<AuthorId>
{
    public override AuthorId Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var value = reader.GetString();

        return new AuthorId(value ?? string.Empty);
    }

    public override void Write(
        Utf8JsonWriter writer,
        AuthorId value,
        JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}
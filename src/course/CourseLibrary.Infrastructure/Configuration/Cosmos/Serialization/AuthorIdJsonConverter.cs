using CourseLibrary.Domain.ValueObjects;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CourseLibrary.Infrastructure.Configuration.Cosmos.Serialization;

public sealed class AuthorIdJsonConverter : JsonConverter<AuthorId>
{
    public override AuthorId Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var value = reader.GetGuid();

        return new AuthorId(value);
    }

    public override void Write(
        Utf8JsonWriter writer,
        AuthorId value,
        JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}
using CourseLibrary.Domain.ValueObjects;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CourseLibrary.Infrastructure.Configuration.Cosmos.Serialization;

public sealed class CodeJsonConverter : JsonConverter<Code>
{
    public override Code Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        return Code.Create(reader.GetString() ?? string.Empty);
    }

    public override void Write(
        Utf8JsonWriter writer,
        Code value,
        JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}
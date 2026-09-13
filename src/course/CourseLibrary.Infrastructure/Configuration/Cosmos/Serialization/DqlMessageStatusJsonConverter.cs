using CourseLibrary.Domain.ValueObjects;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CourseLibrary.Infrastructure.Configuration.Cosmos.Serialization;

public sealed class DqlMessageStatusJsonConverter : JsonConverter<DqlMessageStatus>
{
    public override DqlMessageStatus Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        return new DqlMessageStatus(reader.GetGuid());
    }

    public override void Write(
        Utf8JsonWriter writer,
        DqlMessageStatus value,
        JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}
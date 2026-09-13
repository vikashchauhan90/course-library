using CourseLibrary.Domain.ValueObjects;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CourseLibrary.Infrastructure.Configuration.Cosmos.Serialization;

public sealed class CourseEventTypeJsonConverter : JsonConverter<CourseEventType>
{
    public override CourseEventType Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        return new CourseEventType(reader.GetGuid());
    }

    public override void Write(
        Utf8JsonWriter writer,
        CourseEventType value,
        JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}
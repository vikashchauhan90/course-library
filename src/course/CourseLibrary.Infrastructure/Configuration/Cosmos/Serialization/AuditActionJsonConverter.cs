using CourseLibrary.Domain.ValueObjects;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CourseLibrary.Infrastructure.Configuration.Cosmos.Serialization;

public sealed class AuditActionJsonConverter : JsonConverter<AuditAction>
{
    public override AuditAction Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        return new AuditAction(reader.GetGuid());
    }

    public override void Write(
        Utf8JsonWriter writer,
        AuditAction value,
        JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}
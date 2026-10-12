using CourseLibrary.Domain.ValueObjects;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CourseLibrary.Course.Persistence.Data.Serialization;

public sealed class MoneyJsonConverter : JsonConverter<Money>
{
    public override Money Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        return Money.Create(reader.GetDecimal());
    }

    public override void Write(
        Utf8JsonWriter writer,
        Money value,
        JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value.Amount);
    }
}
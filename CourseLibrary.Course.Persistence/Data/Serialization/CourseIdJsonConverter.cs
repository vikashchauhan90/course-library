using CourseLibrary.Course.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CourseLibrary.Course.Persistence.Data.Serialization;

public sealed class CourseIdJsonConverter : JsonConverter<CourseId>
{
    public override CourseId Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var value = reader.GetString() ?? string.Empty;

        return new CourseId(value);
    }

    public override void Write(
        Utf8JsonWriter writer,
        CourseId value,
        JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}
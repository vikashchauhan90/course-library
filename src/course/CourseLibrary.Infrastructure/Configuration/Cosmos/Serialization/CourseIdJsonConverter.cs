using CourseLibrary.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CourseLibrary.Infrastructure.Configuration.Cosmos.Serialization;

public sealed class CourseIdJsonConverter : JsonConverter<CourseId>
{
    public override CourseId Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var value = reader.GetGuid();

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
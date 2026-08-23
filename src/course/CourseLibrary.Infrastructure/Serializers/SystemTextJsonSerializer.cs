
using CourseLibrary.Application.Abstractions.Serializers;
using CourseLibrary.Infrastructure.Observability.Traces;
using System.Diagnostics;
using System.Text.Json;

namespace CourseLibrary.Infrastructure.Serializers;

public sealed class SystemTextJsonSerializer<T>(
    JsonSerializerOptions options) : ISerializer<T>
{
    public byte[] Serialize(T value)
    {
        ArgumentNullException.ThrowIfNull(value);
        using var activity = ActivitySources.Infrastructure.StartActivity("SystemTextJsonSerializer.Serialize", ActivityKind.Internal);
        activity?.SetTag("type", typeof(T).FullName);
        return JsonSerializer.SerializeToUtf8Bytes(
            value,
            options);
    }

    public T Deserialize(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);
        using var activity = ActivitySources.Infrastructure.StartActivity("SystemTextJsonSerializer.Deserialize", ActivityKind.Internal);
        activity?.SetTag("type", typeof(T).FullName);
        return JsonSerializer.Deserialize<T>(
                   data,
                   options)
               ?? throw new InvalidOperationException(
                   $"Unable to deserialize cached data to {typeof(T).FullName}.");
    }
}
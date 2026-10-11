using CourseLibrary.Platform.Observability.Traces;
using CourseLibrary.Platform.Serialization.Abstractions;
using System.Text.Json;

namespace CourseLibrary.Platform.Serialization.Serializers;

internal sealed class SystemTextJsonSerializer<T>(
    JsonSerializerOptions options) : ISerializer<T>
{
    private static readonly PlatformActivitySource ActivitySources = new();
    public byte[] Serialize(T value)
    {
        ArgumentNullException.ThrowIfNull(value);
        using var activity = ActivitySources.StartActivity("SystemTextJsonSerializer.Serialize");
        activity?.SetTag("type", typeof(T).FullName);
        return JsonSerializer.SerializeToUtf8Bytes(
            value,
            options);
    }

    public T Deserialize(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);
        using var activity = ActivitySources.StartActivity("SystemTextJsonSerializer.Deserialize");
        activity?.SetTag("type", typeof(T).FullName);
        return JsonSerializer.Deserialize<T>(
                   data,
                   options)
               ?? throw new InvalidOperationException(
                   $"Unable to deserialize cached data to {typeof(T).FullName}.");
    }
}
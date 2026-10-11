using CourseLibrary.Platform.Observability.Traces;
using CourseLibrary.Platform.Serialization.Observability;
using CourseLibrary.Platform.Serialization.Abstractions;
using System.Text.Json;

namespace CourseLibrary.Platform.Serialization.Serializers;

internal sealed class SystemTextJsonSerializer<T>(
    JsonSerializerOptions options) : ISerializer<T>
{
    private static readonly PlatformActivitySource ActivitySources = new();
    private static readonly SerializationMeter Metrics = SerializationMeter.Shared;
    public byte[] Serialize(T value)
    {
        ArgumentNullException.ThrowIfNull(value);
        using var activity = ActivitySources.StartActivity("SystemTextJsonSerializer.Serialize");
        activity?.SetTag("type", typeof(T).FullName);
        try
        {
            var result = JsonSerializer.SerializeToUtf8Bytes(value, options);
            Metrics.RecordOperation("system-text-json", "serialize", "success");
            return result;
        }
        catch
        {
            Metrics.RecordOperation("system-text-json", "serialize", "error");
            throw;
        }
    }

    public T Deserialize(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);
        using var activity = ActivitySources.StartActivity("SystemTextJsonSerializer.Deserialize");
        activity?.SetTag("type", typeof(T).FullName);
        try
        {
            var result = JsonSerializer.Deserialize<T>(data, options)
                         ?? throw new InvalidOperationException(
                             $"Unable to deserialize cached data to {typeof(T).FullName}.");
            Metrics.RecordOperation("system-text-json", "deserialize", "success");
            return result;
        }
        catch
        {
            Metrics.RecordOperation("system-text-json", "deserialize", "error");
            throw;
        }
    }
}
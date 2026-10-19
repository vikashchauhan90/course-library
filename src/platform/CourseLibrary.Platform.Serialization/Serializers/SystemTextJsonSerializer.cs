using CourseLibrary.Platform.Serialization.Observability;
using CourseLibrary.Platform.Serialization.Abstractions;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CourseLibrary.Platform.Serialization.Serializers;

internal sealed class SystemTextJsonSerializer<T>(
    JsonSerializerOptions options,
    ILogger<SystemTextJsonSerializer<T>> logger) : ISerializer<T>
{
    private static readonly SerializationActivitySource ActivitySources = SerializationActivitySource.Shared;
    private static readonly SerializationMeter Metrics = SerializationMeter.Shared;

    public byte[] Serialize(T value)
    {
        ArgumentNullException.ThrowIfNull(value);
        using var activity = ActivitySources.StartActivity("SystemTextJsonSerializer.Serialize");
        var typeName = typeof(T).FullName ?? typeof(T).Name;
        activity?.SetTag("type", typeName);
        try
        {
            var result = JsonSerializer.SerializeToUtf8Bytes(value, options);
            Metrics.RecordOperation("system-text-json", "serialize", "success");
            activity?.SetTag("outcome", "success");
            logger.SerializationSucceeded("system-text-json", typeName);
            return result;
        }
        catch (Exception exception)
        {
            Metrics.RecordOperation("system-text-json", "serialize", "error");
            activity?.SetTag("outcome", "error");
            activity?.SetStatus(System.Diagnostics.ActivityStatusCode.Error, "Serialization failed.");
            logger.SerializationFailed(exception, "system-text-json", typeName);
            throw;
        }
    }

    public T Deserialize(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);
        using var activity = ActivitySources.StartActivity("SystemTextJsonSerializer.Deserialize");
        var typeName = typeof(T).FullName ?? typeof(T).Name;
        activity?.SetTag("type", typeName);
        try
        {
            var result = JsonSerializer.Deserialize<T>(data, options)
                         ?? throw new InvalidOperationException(
                             $"Unable to deserialize data to {typeName}.");
            Metrics.RecordOperation("system-text-json", "deserialize", "success");
            activity?.SetTag("outcome", "success");
            logger.DeserializationSucceeded("system-text-json", typeName);
            return result;
        }
        catch (Exception exception)
        {
            Metrics.RecordOperation("system-text-json", "deserialize", "error");
            activity?.SetTag("outcome", "error");
            activity?.SetStatus(System.Diagnostics.ActivityStatusCode.Error, "Deserialization failed.");
            logger.DeserializationFailed(exception, "system-text-json", typeName);
            throw;
        }
    }
}
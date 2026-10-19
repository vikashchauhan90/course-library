using CourseLibrary.Platform.Serialization.Observability;
using CourseLibrary.Platform.Serialization.Abstractions;
using MessagePack;
using MessagePack.Resolvers;
using Microsoft.Extensions.Logging;

namespace CourseLibrary.Platform.Serialization.Serializers;

internal sealed class MessagePackSerializer<T>(
    ILogger<MessagePackSerializer<T>> logger) : ISerializer<T>
{
    private static readonly MessagePackSerializerOptions Options =
        MessagePackSerializerOptions.Standard
            .WithResolver(ContractlessStandardResolver.Instance);

    private static readonly SerializationActivitySource ActivitySources = SerializationActivitySource.Shared;
    private static readonly SerializationMeter Metrics = SerializationMeter.Shared;

    public byte[] Serialize(T value)
    {
        ArgumentNullException.ThrowIfNull(value);
        using var activity = ActivitySources.StartActivity("MessagePackSerializer.Serialize");
        var typeName = typeof(T).FullName ?? typeof(T).Name;
        activity?.SetTag("type", typeName);
        try
        {
            var result = MessagePackSerializer.Serialize(value, Options);
            Metrics.RecordOperation("message-pack", "serialize", "success");
            activity?.SetTag("outcome", "success");
            logger.SerializationSucceeded("message-pack", typeName);
            return result;
        }
        catch (Exception exception)
        {
            Metrics.RecordOperation("message-pack", "serialize", "error");
            activity?.SetTag("outcome", "error");
            activity?.SetStatus(System.Diagnostics.ActivityStatusCode.Error, "Serialization failed.");
            logger.SerializationFailed(exception, "message-pack", typeName);
            throw;
        }
    }

    public T Deserialize(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);
        using var activity = ActivitySources.StartActivity("MessagePackSerializer.Deserialize");
        var typeName = typeof(T).FullName ?? typeof(T).Name;
        activity?.SetTag("type", typeName);
        try
        {
            var result = MessagePackSerializer.Deserialize<T>(data, Options);
            Metrics.RecordOperation("message-pack", "deserialize", "success");
            activity?.SetTag("outcome", "success");
            logger.DeserializationSucceeded("message-pack", typeName);
            return result;
        }
        catch (Exception exception)
        {
            Metrics.RecordOperation("message-pack", "deserialize", "error");
            activity?.SetTag("outcome", "error");
            activity?.SetStatus(System.Diagnostics.ActivityStatusCode.Error, "Deserialization failed.");
            logger.DeserializationFailed(exception, "message-pack", typeName);
            throw;
        }
    }
}
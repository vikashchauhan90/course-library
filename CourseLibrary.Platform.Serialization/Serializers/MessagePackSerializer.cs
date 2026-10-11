using CourseLibrary.Platform.Observability.Traces;
using CourseLibrary.Platform.Serialization.Observability;
using CourseLibrary.Platform.Serialization.Abstractions;
using MessagePack;
using MessagePack.Resolvers;

namespace CourseLibrary.Platform.Serialization.Serializers;

internal sealed class MessagePackSerializer<T> : ISerializer<T>
{
    private static readonly MessagePackSerializerOptions Options =
        MessagePackSerializerOptions.Standard
            .WithResolver(ContractlessStandardResolver.Instance);

    private static readonly PlatformActivitySource ActivitySources = new();
    private static readonly SerializationMeter Metrics = SerializationMeter.Shared;
    public byte[] Serialize(T value)
    {
        ArgumentNullException.ThrowIfNull(value);
        using var activity = ActivitySources.StartActivity("MessagePackSerializer.Serialize");
        activity?.SetTag("type", typeof(T).FullName);
        try
        {
            var result = MessagePackSerializer.Serialize(value, Options);
            Metrics.RecordOperation("message-pack", "serialize", "success");
            return result;
        }
        catch
        {
            Metrics.RecordOperation("message-pack", "serialize", "error");
            throw;
        }
    }

    public T Deserialize(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);
        using var activity = ActivitySources.StartActivity("MessagePackSerializer.Deserialize");
        activity?.SetTag("type", typeof(T).FullName);
        try
        {
            var result = MessagePackSerializer.Deserialize<T>(data, Options);
            Metrics.RecordOperation("message-pack", "deserialize", "success");
            return result;
        }
        catch
        {
            Metrics.RecordOperation("message-pack", "deserialize", "error");
            throw;
        }
    }
}
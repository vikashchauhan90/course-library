using CourseLibrary.Platform.Observability.Traces;
using CourseLibrary.Platform.Serialization.Abstractions;
using MessagePack;
using MessagePack.Resolvers;
using System.Diagnostics;

namespace CourseLibrary.Platform.Serialization.Serializers;

internal sealed class MessagePackSerializer<T> : ISerializer<T>
{
    private static readonly MessagePackSerializerOptions Options =
        MessagePackSerializerOptions.Standard
            .WithResolver(ContractlessStandardResolver.Instance);

    private static readonly PlatformActivitySource ActivitySources = new();
    public byte[] Serialize(T value)
    {
        ArgumentNullException.ThrowIfNull(value);
        using var activity = ActivitySources.StartActivity("MessagePackSerializer.Serialize");
        activity?.SetTag("type", typeof(T).FullName);
        return MessagePackSerializer.Serialize(
            value,
            Options);
    }

    public T Deserialize(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);
        using var activity = ActivitySources.StartActivity("MessagePackSerializer.Deserialize");
        activity?.SetTag("type", typeof(T).FullName);
        return MessagePackSerializer.Deserialize<T>(
            data,
            Options);
    }
}
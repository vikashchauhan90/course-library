using CourseLibrary.Application.Abstractions.Serializers;
using CourseLibrary.Infrastructure.Observability.Traces;
using MessagePack;
using MessagePack.Resolvers;
using System.Diagnostics;

namespace CourseLibrary.Infrastructure.Serializers;

public sealed class MessagePackSerializer<T> : ISerializer<T>
{
    private static readonly MessagePackSerializerOptions Options =
        MessagePackSerializerOptions.Standard
            .WithResolver(ContractlessStandardResolver.Instance);

    public byte[] Serialize(T value)
    {
        ArgumentNullException.ThrowIfNull(value);
        using var activity = ActivitySources.Infrastructure.StartActivity("MessagePackSerializer.Serialize", ActivityKind.Internal);
        activity?.SetTag("type", typeof(T).FullName);
        return MessagePackSerializer.Serialize(
            value,
            Options);
    }

    public T Deserialize(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);
        using var activity = ActivitySources.Infrastructure.StartActivity("MessagePackSerializer.Deserialize", ActivityKind.Internal);
        activity?.SetTag("type", typeof(T).FullName);
        return MessagePackSerializer.Deserialize<T>(
            data,
            Options);
    }
}
using CourseLibrary.Idp.Application.Abstractions.Serializers;
using MessagePack;
using MessagePack.Resolvers;

namespace CourseLibrary.Idp.Infrastructure.Serializers;

public sealed class MessagePackSerializer<T> : ISerializer<T>
{
    private static readonly MessagePackSerializerOptions Options =
        MessagePackSerializerOptions.Standard
            .WithResolver(ContractlessStandardResolver.Instance);

    public byte[] Serialize(T value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return MessagePackSerializer.Serialize(
            value,
            Options);
    }

    public T Deserialize(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);

        return MessagePackSerializer.Deserialize<T>(
            data,
            Options);
    }
}
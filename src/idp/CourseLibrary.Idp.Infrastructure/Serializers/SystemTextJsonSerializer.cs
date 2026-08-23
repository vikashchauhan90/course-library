
using CourseLibrary.Idp.Application.Abstractions.Serializers;
using System.Text.Json;

namespace CourseLibrary.Idp.Infrastructure.Serializers;

public sealed class SystemTextJsonSerializer<T>(
    JsonSerializerOptions options) : ISerializer<T>
{
    public byte[] Serialize(T value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return JsonSerializer.SerializeToUtf8Bytes(
            value,
            options);
    }

    public T Deserialize(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);

        return JsonSerializer.Deserialize<T>(
                   data,
                   options)
               ?? throw new InvalidOperationException(
                   $"Unable to deserialize cached data to {typeof(T).FullName}.");
    }
}
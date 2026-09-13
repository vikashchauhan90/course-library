using Microsoft.Azure.Cosmos;
using System.Text.Json;

namespace CourseLibrary.Infrastructure.Configuration.Cosmos.Serialization;

internal sealed class CourseLibraryCosmosSerializer(
    JsonSerializerOptions options) : CosmosSerializer
{
    public override T FromStream<T>(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        try
        {
            return JsonSerializer.Deserialize<T>(stream, options)
                ?? throw new JsonException(
                    $"Unable to deserialize a Cosmos document as {typeof(T).Name}.");
        }
        finally
        {
            stream.Dispose();
        }
    }

    public override Stream ToStream<T>(T input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var stream = new MemoryStream();
        JsonSerializer.Serialize(stream, input, options);
        stream.Position = 0;

        return stream;
    }
}
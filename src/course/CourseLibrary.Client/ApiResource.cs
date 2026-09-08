using System.Text.Json;
using System.Text.Json.Serialization;
using Hal.Core;

namespace CourseLibrary.Client;

public sealed class HalDocument<T> : IResource<T>
{
    public required T Data { get; init; }
    public ISet<ILink> Links { get; } = new HashSet<ILink>();
    public IDictionary<string, object> EmbeddedResources { get; } = new Dictionary<string, object>();

    public void AddLink(ILink link) => Links.Add(link);

    public void AddEmbeddedResource<TResource>(
        string relation,
        IEmbeddedResource<TResource> resource) =>
        EmbeddedResources[relation] = resource;
}

public sealed class HalEmbeddedResource<T> : IEmbeddedResource<T>
{
    public T Embedded { get; init; } = default!;
}

internal sealed class HalDocumentJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert) =>
        typeToConvert.IsGenericType &&
        typeToConvert.GetGenericTypeDefinition() == typeof(HalDocument<>);

    public override JsonConverter CreateConverter(
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var dataType = typeToConvert.GetGenericArguments()[0];
        var converterType = typeof(HalDocumentJsonConverter<>).MakeGenericType(dataType);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }
}

internal sealed class HalDocumentJsonConverter<T> : JsonConverter<HalDocument<T>>
{
    public override HalDocument<T> Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var root = document.RootElement;

        if (!root.TryGetProperty("data", out var dataElement))
            throw new JsonException("The HAL response does not contain a data property.");

        var links = root.TryGetProperty("links", out var linksElement)
            ? JsonSerializer.Deserialize<List<Link>>(linksElement, options)
            : null;

        var resourceData = JsonSerializer.Deserialize<T>(dataElement, options);

        if (resourceData is null)
            throw new JsonException($"The HAL resource data could not be deserialized as {typeof(T).Name}.");

        var result = new HalDocument<T> { Data = resourceData };
        if (links is not null)
        {
            foreach (var link in links)
                result.AddLink(link);
        }

        if (root.TryGetProperty("embedded", out var embeddedElement) &&
            embeddedElement.ValueKind == JsonValueKind.Object)
        {
            foreach (var embeddedProperty in embeddedElement.EnumerateObject())
            {
                var embeddedResources = JsonSerializer.Deserialize<List<HalEmbeddedResource<JsonElement>>>(
                    embeddedProperty.Value,
                    options);

                if (embeddedResources is not null)
                    result.EmbeddedResources[embeddedProperty.Name] = embeddedResources;
            }
        }

        return result;
    }

    public override void Write(
        Utf8JsonWriter writer,
        HalDocument<T> value,
        JsonSerializerOptions options) =>
        throw new NotSupportedException("HalDocument is a response-only client model.");
}
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using CourseLibrary.Models.Course;

namespace CourseLibrary.Client.Courses;

public sealed class CourseApiException(
    HttpStatusCode statusCode,
    string operation,
    CourseApiProblemDetails? problemDetails,
    string? rawDetail = null) : Exception(
        $"Course API {operation} failed with {(int)statusCode} ({statusCode}). " +
        (problemDetails?.Detail ?? rawDetail ?? "The request failed."))
{
    public HttpStatusCode StatusCode { get; } = statusCode;
    public string Operation { get; } = operation;
    public CourseApiProblemDetails? ProblemDetails { get; } = problemDetails;
    public string? Detail { get; } = problemDetails?.Detail ?? rawDetail;
}

public sealed record CourseApiProblemDetails(
    string? Type,
    int? Status,
    string? Title,
    string? Detail,
    string? Instance,
    string? TraceId,
    DateTimeOffset? Timestamp,
    IReadOnlyDictionary<string, string[]>? Errors);

internal sealed class HalResource<T>
{
    [JsonPropertyName("data")]
    public JsonElement? Data { get; init; }

    [JsonPropertyName("_links")]
    public IReadOnlyDictionary<string, JsonElement>? Links { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement> Properties { get; init; } = [];

    public T ToModel(JsonSerializerOptions options)
    {
        var json = Data is { } data
            ? data.GetRawText()
            : JsonSerializer.Serialize(Properties, options);
        return JsonSerializer.Deserialize<T>(json, options)
            ?? throw new JsonException("The HAL response did not contain a course resource.");
    }
}

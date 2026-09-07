using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Hal.Core;
using Hal.Core.Builders;

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

internal sealed class HalDocument<T>
{
    [JsonPropertyName("data")]
    public T? Data { get; init; }

    [JsonPropertyName("_links")]
    public Dictionary<string, JsonElement>? Links { get; init; }

    public IResource<T> ToResource()
    {
        if (Data is null)
            throw new JsonException("The HAL response did not contain a data payload.");

        IResourceBuilder<T> builder = new ResourceBuilder<T>(Data);
        if (Links is not null)
        {
            foreach (var link in Links)
            {
                if (!link.Value.TryGetProperty("href", out var href))
                    continue;

                var method = HttpVerbs.Get;
                if (link.Value.TryGetProperty("method", out var methodValue) &&
                    Enum.TryParse(methodValue.GetString(), ignoreCase: true, out HttpVerbs parsedMethod))
                {
                    method = parsedMethod;
                }

                builder = builder.AddLink(link.Key, href.GetString() ?? string.Empty, method);
            }
        }

        return builder.Build();
    }
}

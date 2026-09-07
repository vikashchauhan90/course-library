using System.Net;

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

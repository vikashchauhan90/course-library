using System.Net;

namespace CourseLibrary.Client.Courses;

public sealed record CourseDetails(
    string? Id,
    string? Title,
    string? Description,
    string? AuthorId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    AuthorDetails? Author);

public sealed record PageResult<T>(
    IReadOnlyList<T> Items,
    string? ContinuationToken,
    bool HasMore);

public sealed record AuthorDetails(
    string? Id,
    string? Name,
    string? Bio,
    string? Website,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreateCourseRequest(string Title, string Description);

public sealed record UpdateCourseRequest(string Title, string Description);

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

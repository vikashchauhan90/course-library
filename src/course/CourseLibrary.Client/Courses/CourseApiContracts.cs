using System.Net;

namespace CourseLibrary.Client.Courses;

public sealed record CourseDetails(
    string? Id,
    string? Title,
    string? Description,
    string? AuthorId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreateCourseRequest(string Title, string Description);

public sealed record UpdateCourseRequest(string Title, string Description);

public sealed class CourseApiException(
    HttpStatusCode statusCode,
    string operation,
    string detail) : Exception($"Course API {operation} failed with {(int)statusCode} ({statusCode}). {detail}")
{
    public HttpStatusCode StatusCode { get; } = statusCode;
    public string Operation { get; } = operation;
    public string Detail { get; } = detail;
}

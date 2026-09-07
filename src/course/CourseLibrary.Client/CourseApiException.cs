using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Hal.Core;
using Hal.Core.Builders;

namespace CourseLibrary.Client;

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
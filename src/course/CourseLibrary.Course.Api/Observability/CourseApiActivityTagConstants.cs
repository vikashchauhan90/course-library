namespace CourseLibrary.Course.Api.Observability;

internal static class CourseApiActivityTagConstants
{
    public const string EndpointName = "api.endpoint.name";
    public const string EndpointOutcome = "api.endpoint.outcome";
    public const string HttpRequestMethod = "http.request.method";
    public const string HttpResponseStatusCode = "http.response.status_code";
    public const string DurationMilliseconds = "api.endpoint.duration_ms";
    public const string ErrorType = "error.type";
}

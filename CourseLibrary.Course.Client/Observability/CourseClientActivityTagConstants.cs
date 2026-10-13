namespace CourseLibrary.Course.Client.Observability;

public static class CourseClientActivityTagConstants
{
    public const string Operation = "operation";
    public const string HttpRequestMethod = "http.request.method";
    public const string HttpResponseStatusCode = "http.response.status_code";
    public const string OperationSuccess = "course.operation.success";
    public const string OperationCancelled = "course.operation.cancelled";
    public const string FailureType = "failure.type";

    public const string HttpFailure = "http";
    public const string ClientFailure = "client";
    public const string CancelledFailure = "cancelled";
}

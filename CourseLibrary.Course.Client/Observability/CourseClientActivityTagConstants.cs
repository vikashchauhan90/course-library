using System;
using System.Collections.Generic;
using System.Text;

namespace CourseLibrary.Course.Client.Observability;

internal static class CourseClientActivityTagConstants
{
    public const string Operation = "operation.name";
    public const string HttpRequestMethod = "http.request.method";
    public const string HttpResponseStatusCode = "http.response.status_code";

    public const string OperationSuccess = "operation.success";

    public const string HttpFailure = "http.failure";

    public const string FailureType = "failure.type";

    public const string CancelledFailure = "cancelled.failure";
    public const string OperationCancelled = "operation.cancelled";

    public const string ExceptionType = "exception.type";

    public const string ExceptionData = "exception.data";

    public const string ClientFailure = "client.failure";
}

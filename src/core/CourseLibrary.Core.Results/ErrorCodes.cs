namespace CourseLibrary.Core.Results;

public static class ErrorCodes
{
    public const string NotFound = "not_found";

    public const string Validation = "validation";

    public const string Conflict = "conflict";
    public const string Unprocessable = "unprocessable";

    public const string Unauthorized = "unauthorized";

    public const string Forbidden = "forbidden";

    public const string Timeout = "timeout";

    public const string RateLimited = "rate_limited";

    public const string Cancelled = "cancelled";

    public const string Unexpected = "unexpected";
}
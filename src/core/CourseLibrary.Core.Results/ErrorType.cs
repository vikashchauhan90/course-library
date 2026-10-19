namespace CourseLibrary.Core.Results;

public enum ErrorType
{
    None = 0,

    Validation,
    NotFound,
    Conflict,
    Unauthorized,
    Forbidden,

    Timeout,
    RateLimited,
    Cancelled,
    Concurrency,

    ExternalService,
    Infrastructure,

    BusinessRule,

    Unexpected
}
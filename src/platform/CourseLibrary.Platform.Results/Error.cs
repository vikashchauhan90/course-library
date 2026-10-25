namespace CourseLibrary.Platform.Results;

public sealed record Error(
    string Code,
    string Message,
    ErrorType Type,
    IReadOnlyDictionary<string, string[]>? Errors = null)
{

    public static Error Validation(
        string message,
        IReadOnlyDictionary<string, string[]>? errors = null)
        => Create(
            ErrorCodes.Validation,
            message,
            ErrorType.Validation,
            errors);

    public static Error Validation(
        string code,
        string message,
        IReadOnlyDictionary<string, string[]>? errors = null)
        => Create(
            code,
            message,
            ErrorType.Validation,
            errors);

    public static Error Unprocessable(
        string message,
        IReadOnlyDictionary<string, string[]>? errors = null)
        => Create(
            ErrorCodes.Unprocessable,
            message,
            ErrorType.BusinessRule,
            errors);

    public static Error Unprocessable(
       string code,
       string message,
       IReadOnlyDictionary<string, string[]>? errors = null)
       => Create(
           code,
           message,
           ErrorType.BusinessRule,
           errors);

    public static Error NotFound(
        string message,
        IReadOnlyDictionary<string, string[]>? errors = null)
        => Create(
            ErrorCodes.NotFound,
            message,
            ErrorType.NotFound,
            errors);

    public static Error NotFound(
        string code,
       string message,
       IReadOnlyDictionary<string, string[]>? errors = null)
       => Create(
           code,
           message,
           ErrorType.NotFound,
           errors);
    public static Error Conflict(
        string message,
        IReadOnlyDictionary<string, string[]>? errors = null)
        => Create(
            ErrorCodes.Conflict,
            message,
            ErrorType.Conflict,
            errors);

    public static Error Conflict(
       string code,
       string message,
       IReadOnlyDictionary<string, string[]>? errors = null)
       => Create(
           code,
           message,
           ErrorType.Conflict,
           errors);

    public static Error Unauthorized(
        string message,
        IReadOnlyDictionary<string, string[]>? errors = null)
        => Create(
            ErrorCodes.Unauthorized,
            message,
            ErrorType.Unauthorized,
            errors);

    public static Error Unauthorized(
        string code,
        string message,
        IReadOnlyDictionary<string, string[]>? errors = null)
        => Create(
            code,
            message,
            ErrorType.Unauthorized,
            errors);

    public static Error Forbidden(
        string message,
        IReadOnlyDictionary<string, string[]>? errors = null)
        => Create(
            ErrorCodes.Forbidden,
            message,
            ErrorType.Forbidden,
            errors);

    public static Error Forbidden(
       string code,
       string message,
       IReadOnlyDictionary<string, string[]>? errors = null)
       => Create(
           code,
           message,
           ErrorType.Forbidden,
           errors);

    public static Error Timeout(
        string message,
        IReadOnlyDictionary<string, string[]>? errors = null)
        => Create(
            ErrorCodes.Timeout,
            message,
            ErrorType.Timeout,
            errors);

    public static Error Timeout(
       string code,
       string message,
       IReadOnlyDictionary<string, string[]>? errors = null)
       => Create(
           code,
           message,
           ErrorType.Timeout,
           errors);

    public static Error RateLimited(
        string message,
        IReadOnlyDictionary<string, string[]>? errors = null)
        => Create(
            ErrorCodes.RateLimited,
            message,
            ErrorType.RateLimited,
            errors);

    public static Error RateLimited(
      string code,
      string message,
      IReadOnlyDictionary<string, string[]>? errors = null)
      => Create(
          code,
          message,
          ErrorType.RateLimited,
          errors);
    public static Error Cancelled(
        string message = "Operation was cancelled.",
        IReadOnlyDictionary<string, string[]>? errors = null)
        => Create(
            ErrorCodes.Cancelled,
            message,
            ErrorType.Cancelled,
            errors);

    public static Error Cancelled(
        string code,
        string message = "Operation was cancelled.",
        IReadOnlyDictionary<string, string[]>? errors = null)
        => Create(
            code,
            message,
            ErrorType.Cancelled,
            errors);

    public static Error Unexpected(
        string message,
        IReadOnlyDictionary<string, string[]>? errors = null)
        => Create(
            ErrorCodes.Unexpected,
            message,
            ErrorType.Unexpected,
            errors);

    public static Error Unexpected(
        string code,
        string message,
        IReadOnlyDictionary<string, string[]>? errors = null)
        => Create(
            code,
            message,
            ErrorType.Unexpected,
            errors);

    public static Error Create(
        string code,
        string message,
        ErrorType type,
        IReadOnlyDictionary<string, string[]>? errors = null)
        => new(
            code,
            message,
            type,
            errors);
}
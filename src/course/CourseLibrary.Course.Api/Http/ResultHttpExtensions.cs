using CourseLibrary.Core.Results;

namespace CourseLibrary.Course.Api.Http;

internal static class ResultHttpExtensions
{
    public static IResult ToHttpResult<T>(this Result<T> result, Func<T?, IResult>? onSuccess = null)
    {
        ArgumentNullException.ThrowIfNull(result);
        if (result.IsSuccess)
        {
            return onSuccess?.Invoke(result.Value) ?? Results.Ok(result.Value);
        }

        var error = result.Error;
        if (error is null)
        {
            return Results.Problem(statusCode: StatusCodes.Status500InternalServerError);
        }

        var statusCode = error.Type switch
        {
            ErrorType.Validation  => StatusCodes.Status400BadRequest,
            ErrorType.BusinessRule => StatusCodes.Status422UnprocessableEntity,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict or ErrorType.Concurrency => StatusCodes.Status409Conflict,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorType.Timeout => StatusCodes.Status504GatewayTimeout,
            ErrorType.RateLimited => StatusCodes.Status429TooManyRequests,
            ErrorType.ExternalService => StatusCodes.Status503ServiceUnavailable,
            ErrorType.Cancelled => StatusCodes.Status408RequestTimeout,
            _ => StatusCodes.Status500InternalServerError
        };

        if (error.Type == ErrorType.Validation && error.Errors is { Count: > 0 })
        {
            return Results.ValidationProblem(
                error.Errors,
                detail: error.Message,
                statusCode: statusCode,
                extensions: new Dictionary<string, object?> { ["code"] = error.Code });
        }

        return Results.Problem(
            title: error.Code,
            detail: error.Message,
            statusCode: statusCode);
    }
}

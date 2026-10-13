using CourseLibrary.Core.Domain.Exceptions;
using CourseLibrary.Core.Results;

namespace CourseLibrary.Course.Application.Core;

internal static class DomainExceptionErrorMapper
{
    public static Error ToError(DomainException exception)
        => exception switch
        {
            BadRequestException => Error.Validation(exception.Message),
            ValidationFailedException => Error.Unprocessable(exception.Message),
            DataNotFoundException => Error.NotFound(exception.Message),
            DataConflictException => Error.Conflict(exception.Message),
            ForbiddenException => Error.Forbidden(exception.Message),
            UnauthorizedException => Error.Unauthorized(exception.Message),
            DataConcurrencyException => Error.Create(
                "concurrency",
                exception.Message,
                ErrorType.Concurrency),
            DataServiceTimeoutException => Error.Timeout(exception.Message),
            DataThrottledException or TooManyRequestsException => Error.RateLimited(exception.Message),
            DataServiceUnavailableException => Error.Create(
                "service_unavailable",
                exception.Message,
                ErrorType.ExternalService),
            DataAccessException => Error.Create(
                "data_access",
                exception.Message,
                ErrorType.Infrastructure),
            _ => Error.Create(
                "domain_error",
                exception.Message,
                ErrorType.BusinessRule)
        };
}

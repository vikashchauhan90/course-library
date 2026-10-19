using CourseLibrary.Core.Domain.Exceptions;
using System.Net;

namespace CourseLibrary.Course.Api.Http;

internal static class DomainExceptionExtensions
{
    public static ExceptionMapping MapException(this DomainException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        var statusCode = exception.ToHttpStatus();
        var title = GetTitle((int)statusCode);
        var type = GetProblemType((int)statusCode);
        var detail = exception.Message;
        var errors = exception switch
        {
            BadRequestException ex => ex.Errors,
            ValidationFailedException ex => ex.Errors,
            _ => null
        };

        return new ExceptionMapping(
            StatusCode: (int)statusCode,
            Title: title,
            Detail: detail,
            Type: type,
            Errors: errors);
    }
    public static HttpStatusCode ToHttpStatus(this DomainException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return exception switch
        {
            BadRequestException => HttpStatusCode.BadRequest,
            ValidationFailedException => HttpStatusCode.UnprocessableEntity,
            DataConflictException => HttpStatusCode.Conflict,
            DataConcurrencyException => HttpStatusCode.PreconditionFailed,
            DataThrottledException => HttpStatusCode.TooManyRequests,
            DataServiceUnavailableException => HttpStatusCode.ServiceUnavailable,
            DataServiceTimeoutException => HttpStatusCode.RequestTimeout,
            UnauthorizedException => HttpStatusCode.Unauthorized,
            ForbiddenException => HttpStatusCode.Forbidden,
            DataNotFoundException => HttpStatusCode.NotFound,
            DataAccessException => HttpStatusCode.InternalServerError,
            _ => HttpStatusCode.InternalServerError
        };
    }

    public static string GetProblemType(int statusCode)
    {
        return statusCode switch
        {
            // 4xx Client Errors
            StatusCodes.Status400BadRequest =>
                "https://httpstatuses.com/400",

            StatusCodes.Status401Unauthorized =>
                "https://httpstatuses.com/401",

            StatusCodes.Status403Forbidden =>
                "https://httpstatuses.com/403",

            StatusCodes.Status404NotFound =>
                "https://httpstatuses.com/404",

            StatusCodes.Status405MethodNotAllowed =>
                "https://httpstatuses.com/405",

            StatusCodes.Status406NotAcceptable =>
                "https://httpstatuses.com/406",

            StatusCodes.Status408RequestTimeout =>
                "https://httpstatuses.com/408",

            StatusCodes.Status409Conflict =>
                "https://httpstatuses.com/409",

            StatusCodes.Status410Gone =>
                "https://httpstatuses.com/410",

            StatusCodes.Status411LengthRequired =>
                "https://httpstatuses.com/411",

            StatusCodes.Status412PreconditionFailed =>
                "https://httpstatuses.com/412",

            StatusCodes.Status413PayloadTooLarge =>
                "https://httpstatuses.com/413",

            StatusCodes.Status414UriTooLong =>
                "https://httpstatuses.com/414",

            StatusCodes.Status415UnsupportedMediaType =>
                "https://httpstatuses.com/415",

            StatusCodes.Status416RangeNotSatisfiable =>
                "https://httpstatuses.com/416",

            StatusCodes.Status417ExpectationFailed =>
                "https://httpstatuses.com/417",

            StatusCodes.Status421MisdirectedRequest =>
                "https://httpstatuses.com/421",

            StatusCodes.Status422UnprocessableEntity =>
                "https://httpstatuses.com/422",

            StatusCodes.Status423Locked =>
                "https://httpstatuses.com/423",

            StatusCodes.Status424FailedDependency =>
                "https://httpstatuses.com/424",

            StatusCodes.Status428PreconditionRequired =>
                "https://httpstatuses.com/428",

            StatusCodes.Status429TooManyRequests =>
                "https://httpstatuses.com/429",

            StatusCodes.Status431RequestHeaderFieldsTooLarge =>
                "https://httpstatuses.com/431",

            // 5xx Server Errors
            StatusCodes.Status500InternalServerError =>
                "https://httpstatuses.com/500",

            StatusCodes.Status501NotImplemented =>
                "https://httpstatuses.com/501",

            StatusCodes.Status502BadGateway =>
                "https://httpstatuses.com/502",

            StatusCodes.Status503ServiceUnavailable =>
                "https://httpstatuses.com/503",

            StatusCodes.Status504GatewayTimeout =>
                "https://httpstatuses.com/504",

            StatusCodes.Status505HttpVersionNotsupported =>
                "https://httpstatuses.com/505",

            StatusCodes.Status507InsufficientStorage =>
                "https://httpstatuses.com/507",

            StatusCodes.Status508LoopDetected =>
                "https://httpstatuses.com/508",

            _ =>
                "https://httpstatuses.com/500"
        };
    }
    public static string GetTitle(int statusCode)
    {
        return statusCode switch
        {
            // 4xx Client Errors
            StatusCodes.Status400BadRequest =>
                "Bad Request",

            StatusCodes.Status401Unauthorized =>
                "Unauthorized",

            StatusCodes.Status403Forbidden =>
                "Forbidden",

            StatusCodes.Status404NotFound =>
                "Not Found",

            StatusCodes.Status405MethodNotAllowed =>
                "Method Not Allowed",

            StatusCodes.Status406NotAcceptable =>
                "Not Acceptable",

            StatusCodes.Status408RequestTimeout =>
                "Request Timeout",

            StatusCodes.Status409Conflict =>
                "Conflict",

            StatusCodes.Status410Gone =>
                "Gone",

            StatusCodes.Status411LengthRequired =>
                "Length Required",

            StatusCodes.Status412PreconditionFailed =>
                "Precondition Failed",

            StatusCodes.Status413PayloadTooLarge =>
                "Payload Too Large",

            StatusCodes.Status414UriTooLong =>
                "URI Too Long",

            StatusCodes.Status415UnsupportedMediaType =>
                "Unsupported Media Type",

            StatusCodes.Status416RangeNotSatisfiable =>
                "Range Not Satisfiable",

            StatusCodes.Status417ExpectationFailed =>
                "Expectation Failed",

            StatusCodes.Status421MisdirectedRequest =>
                "Misdirected Request",

            StatusCodes.Status422UnprocessableEntity =>
                "Validation Failed",

            StatusCodes.Status423Locked =>
                "Locked",

            StatusCodes.Status424FailedDependency =>
                "Failed Dependency",

            StatusCodes.Status428PreconditionRequired =>
                "Precondition Required",

            StatusCodes.Status429TooManyRequests =>
                "Too Many Requests",

            StatusCodes.Status431RequestHeaderFieldsTooLarge =>
                "Request Header Fields Too Large",

            // 5xx Server Errors
            StatusCodes.Status500InternalServerError =>
                "Internal Server Error",

            StatusCodes.Status501NotImplemented =>
                "Not Implemented",

            StatusCodes.Status502BadGateway =>
                "Bad Gateway",

            StatusCodes.Status503ServiceUnavailable =>
                "Service Unavailable",

            StatusCodes.Status504GatewayTimeout =>
                "Gateway Timeout",

            StatusCodes.Status505HttpVersionNotsupported =>
                "HTTP Version Not Supported",

            StatusCodes.Status507InsufficientStorage =>
                "Insufficient Storage",

            StatusCodes.Status508LoopDetected =>
                "Loop Detected",

            _ when statusCode >= 500 =>
                "Internal Server Error",

            _ =>
                "Request Failed"
        };
    }
}

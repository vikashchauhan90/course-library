using Microsoft.AspNetCore.Http;

namespace CourseLibrary.Domain.Exceptions;

public sealed class TooManyRequestsException : BaseException
{
    public TooManyRequestsException(
        string message = "Too many requests. Please try again later.")
        : base(
            message,
            StatusCodes.Status429TooManyRequests)
    {
    }
}
using Microsoft.AspNetCore.Http;

namespace CourseLibrary.Domain.Exceptions;

public sealed class DataThrottledException : BaseException
{
    public DataThrottledException(
        string message,
        Exception? innerException = null)
        : base(
            message,
            StatusCodes.Status503ServiceUnavailable,
            innerException)
    {
    }
}
using Microsoft.AspNetCore.Http;

namespace CourseLibrary.Domain.Exceptions;

public sealed class DataServiceUnavailableException : BaseException
{
    public DataServiceUnavailableException(
        string message,
        Exception? innerException = null)
        : base(
            message,
            StatusCodes.Status503ServiceUnavailable,
            innerException)
    {
    }
}

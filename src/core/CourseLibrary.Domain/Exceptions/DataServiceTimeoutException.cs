using Microsoft.AspNetCore.Http;

namespace CourseLibrary.Domain.Exceptions;

public sealed class DataServiceTimeoutException : BaseException
{
    public DataServiceTimeoutException(
        string message,
        Exception? innerException = null)
        : base(
            message,
            StatusCodes.Status504GatewayTimeout,
            innerException)
    {
    }
}
using Microsoft.AspNetCore.Http;

namespace CourseLibrary.Domain.Exceptions;

public sealed class DataRequestException : BaseException
{
    public DataRequestException(
        string message,
        Exception? innerException = null)
        : base(
            message,
            StatusCodes.Status400BadRequest,
            innerException)
    {
    }
}
using Microsoft.AspNetCore.Http;

namespace CourseLibrary.Domain.Exceptions;

public sealed class DataConflictException : BaseException
{
    public DataConflictException(
        string message,
        Exception? innerException = null)
        : base(
            message,
            StatusCodes.Status409Conflict,
            innerException)
    {
    }
}
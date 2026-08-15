using Microsoft.AspNetCore.Http;

namespace CourseLibrary.Domain.Exceptions;

public sealed class DataAccessException : BaseException
{
    public DataAccessException(
        string message,
        Exception? innerException = null)
        : base(
            message,
            StatusCodes.Status500InternalServerError,
            innerException)
    {
    }
}
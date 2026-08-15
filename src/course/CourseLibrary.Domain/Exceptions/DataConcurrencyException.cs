using Microsoft.AspNetCore.Http;

namespace CourseLibrary.Domain.Exceptions;

public sealed class DataConcurrencyException : BaseException
{
    public DataConcurrencyException(
        string message,
        Exception? innerException = null)
        : base(
            message,
            StatusCodes.Status409Conflict,
            innerException)
    {
    }
}
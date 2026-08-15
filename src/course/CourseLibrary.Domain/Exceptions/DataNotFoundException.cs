using Microsoft.AspNetCore.Http;

namespace CourseLibrary.Domain.Exceptions;

public sealed class DataNotFoundException : BaseException
{
    public DataNotFoundException(
        string message,
        Exception? innerException = null)
        : base(
            message,
            StatusCodes.Status404NotFound,
            innerException)
    {
    }
}

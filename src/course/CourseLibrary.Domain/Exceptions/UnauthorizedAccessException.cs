using Microsoft.AspNetCore.Http;

namespace CourseLibrary.Domain.Exceptions;

public sealed class UnauthorizedAccessException: BaseException
{
    public UnauthorizedAccessException(
        string message = "You are not authorized to perform this action.")
        : base(
            message,
            StatusCodes.Status403Forbidden)
    {
    }
}

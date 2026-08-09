using Microsoft.AspNetCore.Http;

namespace CourseLibrary.Domain.Exceptions;

public sealed class ForbiddenException : BaseException
{
    public ForbiddenException(
        string message = "You do not have permission to perform this action.")
        : base(
            message,
            StatusCodes.Status403Forbidden)
    {
    }
}
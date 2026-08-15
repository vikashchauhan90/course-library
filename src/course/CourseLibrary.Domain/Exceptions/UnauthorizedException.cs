using Microsoft.AspNetCore.Http;

namespace CourseLibrary.Domain.Exceptions;

public sealed class UnauthorizedException : BaseException
{
    public UnauthorizedException(
        string message = "Authentication is required.")
        : base(
            message,
            StatusCodes.Status401Unauthorized)
    {
    }
}
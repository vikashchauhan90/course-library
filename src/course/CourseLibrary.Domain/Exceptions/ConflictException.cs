
using Microsoft.AspNetCore.Http;

namespace CourseLibrary.Domain.Exceptions;

public sealed class ConflictException : BaseException
{
    public ConflictException(string message)
        : base(
            message,
            StatusCodes.Status409Conflict)
    {
    }
}
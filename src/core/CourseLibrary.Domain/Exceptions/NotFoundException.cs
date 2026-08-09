
using Microsoft.AspNetCore.Http;

namespace CourseLibrary.Domain.Exceptions;

public sealed class NotFoundException : BaseException
{
    public NotFoundException(string message)
        : base(
            message,
            StatusCodes.Status404NotFound)
    {
    }
}
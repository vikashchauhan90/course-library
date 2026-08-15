
using Microsoft.AspNetCore.Http;

namespace CourseLibrary.Domain.Exceptions;

public sealed class BadRequestException : BaseException
{
    public BadRequestException(
        string message = "The request is invalid.")
        : base(
            message,
            StatusCodes.Status400BadRequest)
    {
    }
}

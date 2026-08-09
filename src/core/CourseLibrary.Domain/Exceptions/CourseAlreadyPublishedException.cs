using Microsoft.AspNetCore.Http;

namespace CourseLibrary.Domain.Exceptions;

public sealed class CourseAlreadyPublishedException : BaseException
{
    public CourseAlreadyPublishedException()
        : base(
            "The course has already been published.",
            StatusCodes.Status409Conflict)
    {
    }
}
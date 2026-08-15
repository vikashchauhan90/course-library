using Microsoft.AspNetCore.Http;

namespace CourseLibrary.Domain.Exceptions;

public sealed class CourseCannotBePublishedException : BaseException
{
    public CourseCannotBePublishedException()
        : base(
            "A course cannot be published without at least one lesson.",
            StatusCodes.Status409Conflict)
    {
    }
}
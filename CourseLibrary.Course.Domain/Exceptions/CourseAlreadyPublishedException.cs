using CourseLibrary.Core.Domain.Exceptions;

namespace CourseLibrary.Course.Domain.Exceptions;

public sealed class CourseAlreadyPublishedException : BaseException
{
    public CourseAlreadyPublishedException()
        : base(
            "The course has already been published.",
            409)
    {
    }
}
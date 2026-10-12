using CourseLibrary.Core.Domain.Exceptions;

namespace CourseLibrary.Course.Domain.Exceptions;

public sealed class CourseCannotBePublishedException : BaseException
{
    public CourseCannotBePublishedException()
        : base(
            "A course cannot be published without at least one lesson.",
            409)
    {
    }
}
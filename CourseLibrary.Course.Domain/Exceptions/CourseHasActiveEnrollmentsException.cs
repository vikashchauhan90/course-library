using CourseLibrary.Core.Domain.Exceptions;

namespace CourseLibrary.Course.Domain.Exceptions;

public sealed class CourseHasActiveEnrollmentsException : BaseException
{
    public CourseHasActiveEnrollmentsException()
        : base(
            "The course cannot be deleted while it has active enrollments.",
            409)
    {
    }
}
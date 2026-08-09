using Microsoft.AspNetCore.Http;

namespace CourseLibrary.Domain.Exceptions;

public sealed class CourseHasActiveEnrollmentsException : BaseException
{
    public CourseHasActiveEnrollmentsException()
        : base(
            "The course cannot be deleted while it has active enrollments.",
            StatusCodes.Status409Conflict)
    {
    }
}
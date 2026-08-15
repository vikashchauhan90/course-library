
using Microsoft.AspNetCore.Http;

namespace CourseLibrary.Domain.Exceptions;

public sealed class ValidationFailedException : BaseException
{
    public ValidationFailedException(
        string message = "One or more validation errors occurred.")
        : base(
            message,
            StatusCodes.Status422UnprocessableEntity)
    {
    }

    public IDictionary<string, string[]> Errors { get; init; }
        = new Dictionary<string, string[]>();
}

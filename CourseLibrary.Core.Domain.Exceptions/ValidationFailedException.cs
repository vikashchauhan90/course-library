namespace CourseLibrary.Core.Domain.Exceptions;

public sealed class ValidationFailedException(
    string message = "One or more validation errors occurred.",
    Exception? innerException = null)
    : DomainException(message, innerException)
{
    public IDictionary<string, string[]> Errors { get; init; } = new Dictionary<string, string[]>();
}

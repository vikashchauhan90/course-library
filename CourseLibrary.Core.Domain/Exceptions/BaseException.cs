namespace CourseLibrary.Core.Domain.Exceptions;

public abstract class BaseException(
    string message,
    int statusCode,
    Exception? innerException = null)
    : DomainException(message, innerException)
{
    public int StatusCode { get; } = statusCode;
}

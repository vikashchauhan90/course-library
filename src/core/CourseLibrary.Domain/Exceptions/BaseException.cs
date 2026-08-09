namespace CourseLibrary.Domain.Exceptions;

public abstract class BaseException : Exception
{
    protected BaseException(
        string message,
        int statusCode,
        Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
    }

    public int StatusCode { get; }
}
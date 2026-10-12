namespace CourseLibrary.Core.Domain.Exceptions;

public sealed class DataNotFoundException(
    string message,
    Exception? innerException = null)
    : BaseException(message, 404, innerException);

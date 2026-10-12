namespace CourseLibrary.Core.Domain.Exceptions;

public sealed class DataAccessException(
    string message,
    Exception? innerException = null)
    : BaseException(message, 500, innerException);

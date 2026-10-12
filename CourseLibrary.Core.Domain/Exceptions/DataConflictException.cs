namespace CourseLibrary.Core.Domain.Exceptions;

public sealed class DataConflictException(
    string message,
    Exception? innerException = null)
    : BaseException(message, 409, innerException);

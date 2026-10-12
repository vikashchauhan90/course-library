namespace CourseLibrary.Core.Domain.Exceptions;

public sealed class DataServiceTimeoutException(
    string message,
    Exception? innerException = null)
    : BaseException(message, 504, innerException);

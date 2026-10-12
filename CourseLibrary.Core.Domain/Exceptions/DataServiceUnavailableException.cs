namespace CourseLibrary.Core.Domain.Exceptions;

public sealed class DataServiceUnavailableException(
    string message,
    Exception? innerException = null)
    : BaseException(message, 503, innerException);

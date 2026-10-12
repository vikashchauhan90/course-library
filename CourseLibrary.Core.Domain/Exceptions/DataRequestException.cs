namespace CourseLibrary.Core.Domain.Exceptions;

public sealed class DataRequestException(
    string message,
    Exception? innerException = null)
    : BaseException(message, 400, innerException);

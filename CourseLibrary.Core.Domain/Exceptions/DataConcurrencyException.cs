namespace CourseLibrary.Core.Domain.Exceptions;

public sealed class DataConcurrencyException(
    string message,
    Exception? innerException = null)
    : BaseException(message, 409, innerException);

namespace CourseLibrary.Core.Domain.Exceptions;

public sealed class DataAccessException(
    string message,
    Exception? innerException = null)
    : DomainException(message, innerException);

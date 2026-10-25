namespace CourseLibrary.Platform.Exceptions;

public sealed class DataNotFoundException(
    string message = "The requested resource was not found.",
    Exception? innerException = null)
    : DomainException(message, innerException);

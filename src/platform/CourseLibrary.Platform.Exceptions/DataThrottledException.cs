namespace CourseLibrary.Platform.Exceptions;

public sealed class DataThrottledException(
    string message = "The request has been throttled.",
    Exception? innerException = null)
    : DomainException(message, innerException);

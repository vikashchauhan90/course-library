namespace CourseLibrary.Platform.Exceptions;

public sealed class TooManyRequestsException(
    string message = "Too many requests. Please try again later.")
    : DomainException(message);

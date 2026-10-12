namespace CourseLibrary.Core.Domain.Exceptions;

public sealed class TooManyRequestsException(
    string message = "Too many requests. Please try again later.")
    : BaseException(message, 429);

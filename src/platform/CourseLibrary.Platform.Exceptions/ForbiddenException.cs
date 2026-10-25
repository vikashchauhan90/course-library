namespace CourseLibrary.Platform.Exceptions;

public sealed class ForbiddenException(
    string message = "You do not have permission to perform this action.")
    : DomainException(message);

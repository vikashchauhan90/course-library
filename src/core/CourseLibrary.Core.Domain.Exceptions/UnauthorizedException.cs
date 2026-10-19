namespace CourseLibrary.Core.Domain.Exceptions;

public sealed class UnauthorizedException(
    string message = "Authentication is required.")
    : DomainException(message);

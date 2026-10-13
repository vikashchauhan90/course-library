namespace CourseLibrary.Core.Domain.Exceptions;

public sealed class BadRequestException(
    string message = "The request is invalid.",
    Exception? innerException = null)
    : DomainException(message, innerException);

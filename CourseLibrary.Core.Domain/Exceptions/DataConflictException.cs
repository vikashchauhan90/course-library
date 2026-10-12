namespace CourseLibrary.Core.Domain.Exceptions;

public sealed class DataConflictException(
    string message = "The request conflicts with the current state of the resource.",
    Exception? innerException = null)
    : DomainException(message, innerException);

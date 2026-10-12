namespace CourseLibrary.Core.Domain.Exceptions;

public sealed class ConflictException(string message)
    : BaseException(message, 409);

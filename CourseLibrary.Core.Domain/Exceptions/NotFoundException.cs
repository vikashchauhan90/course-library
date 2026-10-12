namespace CourseLibrary.Core.Domain.Exceptions;

public sealed class NotFoundException(string message)
    : BaseException(message, 404);

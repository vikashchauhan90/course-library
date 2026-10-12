namespace CourseLibrary.Core.Domain.Exceptions;

public sealed class ForbiddenException(
    string message = "You do not have permission to perform this action.")
    : BaseException(message, 403);

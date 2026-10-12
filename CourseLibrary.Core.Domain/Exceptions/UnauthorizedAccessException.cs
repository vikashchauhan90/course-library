namespace CourseLibrary.Core.Domain.Exceptions;

public sealed class UnauthorizedAccessException(
    string message = "You are not authorized to perform this action.")
    : BaseException(message, 403);

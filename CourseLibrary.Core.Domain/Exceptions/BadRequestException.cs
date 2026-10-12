namespace CourseLibrary.Core.Domain.Exceptions;

public sealed class BadRequestException(
    string message = "The request is invalid.")
    : BaseException(message, 400);

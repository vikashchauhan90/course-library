namespace CourseLibrary.Core.Domain.Exceptions;

public sealed class DataServiceTimeoutException(
    string message = "The request to the data service timed out.",
    Exception? innerException = null)
    : DomainException(message, innerException);

namespace CourseLibrary.Core.Domain.Exceptions;

public sealed class DataServiceUnavailableException(
    string message = "The data service is currently unavailable.",
    Exception? innerException = null)
    : DomainException(message, innerException);

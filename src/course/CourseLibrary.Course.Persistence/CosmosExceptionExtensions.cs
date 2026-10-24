using System.Net;
using CourseLibrary.Core.Domain.Exceptions;
using Microsoft.Azure.Cosmos;


namespace CourseLibrary.Course.Persistence;

public static class CosmosExceptionExtensions
{
    public static DomainException ToApplicationException(
        this CosmosException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return exception.StatusCode switch
        {
            HttpStatusCode.Conflict =>
                new DataConflictException(
                    "The data operation caused a conflict.",
                    exception),

            HttpStatusCode.PreconditionFailed =>
                new DataConcurrencyException(
                    "The resource was modified by another operation.",
                    exception),

            HttpStatusCode.TooManyRequests =>
                new DataThrottledException(
                    "The data service is temporarily throttling requests.",
                    exception),

            HttpStatusCode.ServiceUnavailable =>
                new DataServiceUnavailableException(
                    "The data service is temporarily unavailable.",
                    exception),

            HttpStatusCode.RequestTimeout =>
                new DataServiceTimeoutException(
                    "The data service did not respond within the expected time.",
                    exception),

            HttpStatusCode.BadRequest =>
                new BadRequestException(
                    "The data service rejected the request.",
                    exception),

            HttpStatusCode.UnprocessableEntity =>
                new ValidationFailedException(
                    "The data service rejected the request due to validation errors.",
                    exception),

            _ =>
                new DataAccessException(
                    "An error occurred while accessing the data service.",
                    exception)
        };
    }
}
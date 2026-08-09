using CourseLibrary.Domain.Exceptions;
using Microsoft.Azure.Cosmos;
using System.Net;

namespace CourseLibrary.Infrastructure.Cosmos.Extensions;

internal static class CosmosExceptionExtensions
{
    public static BaseException ToApplicationException(
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
                new DataRequestException(
                    "The data service rejected the request.",
                    exception),

            _ =>
                new DataAccessException(
                    "An error occurred while accessing the data service.",
                    exception)
        };
    }
}
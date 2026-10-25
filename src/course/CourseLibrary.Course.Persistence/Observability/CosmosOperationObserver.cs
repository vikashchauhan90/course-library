using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CourseLibrary.Course.Persistence.Observability;

internal static class CosmosOperationObserver
{
    private static readonly CosmosMeter Meter = CosmosMeter.Shared;

    public static void RecordSuccess(
        Activity? activity,
        ILogger logger,
        string operation,
        string containerName,
        long startedAt,
        double requestCharge)
    {
        activity?.RecordSuccess(requestCharge);

        Meter.RecordOperation(
            operation,
            "success",
            Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds,
            requestCharge);

        logger.OperationCompleted(
            operation,
            containerName,
            requestCharge);
    }

    public static void RecordNotFound(
        Activity? activity,
        ILogger logger,
        string operation,
        string containerName,
        long startedAt,
        double requestCharge,
        string id)
    {
        activity?.RecordNotFound();

        Meter.RecordOperation(
            operation,
            "not_found",
            Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds,
            requestCharge);

        logger.DocumentNotFound(
            operation,
            containerName,
            id);
    }

    public static void RecordFailure(
        Activity? activity,
        ILogger logger,
        string operation,
        string containerName,
        long startedAt,
        Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        activity?.RecordFailure(exception);

        var isCancellation = exception is OperationCanceledException;
        var failureType = isCancellation
            ? "cancelled"
            : exception.GetType().Name;

        var requestCharge = exception is CosmosException cosmosException
            ? cosmosException.RequestCharge
            : (double?)null;

        Meter.RecordOperation(
            operation,
            isCancellation ? "cancelled" : "failure",
            Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds,
            requestCharge);

        Meter.RecordFailure(operation, failureType);

        if (isCancellation)
        {
            logger.OperationCancelled(
                operation,
                containerName,
                exception);

            return;
        }

        if (exception is CosmosException cosmosError)
        {
            var statusCode = (int)cosmosError.StatusCode;

            if (statusCode >= 500)
            {
                logger.OperationError(
                    operation,
                    containerName,
                    statusCode,
                    cosmosError.ActivityId,
                    cosmosError.RequestCharge,
                    cosmosError);
            }
            else
            {
                logger.OperationWarning(
                    operation,
                    containerName,
                    statusCode,
                    cosmosError.ActivityId,
                    cosmosError.RequestCharge,
                    cosmosError);
            }

            return;
        }

        logger.UnexpectedOperationFailure(
            operation,
            containerName,
            exception);
    }
}

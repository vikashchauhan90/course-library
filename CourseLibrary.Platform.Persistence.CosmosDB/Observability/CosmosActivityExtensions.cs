using System.Diagnostics;
using Microsoft.Azure.Cosmos;

namespace CourseLibrary.Platform.Persistence.CosmosDB.Observability;

internal static class CosmosActivityExtensions
{
    public static void SetCosmosOperation(
        this Activity? activity,
        string operation,
        string containerName,
        string databaseName)
    {
        activity?.SetTag(CosmosActivityTagConstants.DbSystem, "cosmosdb");
        activity?.SetTag(CosmosActivityTagConstants.DbOperationName, operation);
        activity?.SetTag(CosmosActivityTagConstants.DbNamespace, databaseName);
        activity?.SetTag(CosmosActivityTagConstants.ContainerName, containerName);
    }

    public static void RecordSuccess(
        this Activity? activity,
        double requestCharge)
    {
        activity?.SetTag(CosmosActivityTagConstants.RequestCharge, requestCharge);
        activity?.SetTag(CosmosActivityTagConstants.OperationSuccess, true);
        activity?.SetStatus(ActivityStatusCode.Ok);
    }

    public static void RecordNotFound(this Activity? activity)
    {
        activity?.SetTag("http.response.status_code", 404);
        activity?.SetStatus(ActivityStatusCode.Ok);
    }

    public static void RecordFailure(
        this Activity? activity,
        Exception exception)
    {
        activity?.SetTag(CosmosActivityTagConstants.ErrorType, exception.GetType().Name);

        if (exception is CosmosException cosmosException)
        {
            activity?.SetTag("http.response.status_code", (int)cosmosException.StatusCode);
            activity?.SetTag(CosmosActivityTagConstants.ActivityId, cosmosException.ActivityId);
            activity?.SetTag(CosmosActivityTagConstants.RequestCharge, cosmosException.RequestCharge);
        }

        if (exception is OperationCanceledException)
        {
            activity?.SetTag(CosmosActivityTagConstants.OperationCancelled, true);
        }

        activity?.SetStatus(ActivityStatusCode.Error, exception.Message);
    }
}

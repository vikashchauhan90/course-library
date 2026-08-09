using System.Diagnostics;
using Microsoft.Azure.Cosmos;

namespace CourseLibrary.Infrastructure.Cosmos.Extensions;

internal static class CosmosActivityExtensions
{
    public static void SetCosmosOperation(
        this Activity? activity,
        string operation,
        string containerName)
    {
        if (activity is null)
        {
            return;
        }

        activity.SetTag("db.system", "cosmosdb");
        activity.SetTag("db.operation.name", operation);
        activity.SetTag("db.namespace", containerName);
    }

    public static void RecordSuccess(
        this Activity? activity,
        double requestCharge)
    {
        if (activity is null)
        {
            return;
        }

        activity.SetTag(
            "cosmos.request_charge",
            requestCharge);

        activity.SetStatus(
            ActivityStatusCode.Ok);
    }

    public static void RecordFailure(
        this Activity? activity,
        CosmosException exception)
    {
        if (activity is null)
        {
            return;
        }

        activity.SetTag(
            "http.response.status_code",
            (int)exception.StatusCode);

        activity.SetTag(
            "cosmos.activity_id",
            exception.ActivityId);

        activity.SetTag(
            "cosmos.request_charge",
            exception.RequestCharge);

        activity.SetTag(
            "error.type",
            exception.GetType().Name);

        activity.SetStatus(
            ActivityStatusCode.Error,
            exception.Message);
    }
}
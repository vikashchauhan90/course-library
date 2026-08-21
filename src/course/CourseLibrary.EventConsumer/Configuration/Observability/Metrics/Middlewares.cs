using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CourseLibrary.EventConsumer.Configuration.Observability.Metrics.Middlewares;

public sealed class FunctionMetricsMiddleware(
    ILogger<FunctionMetricsMiddleware> logger)
    : IFunctionsWorkerMiddleware
{
    public async Task Invoke(
        FunctionContext context,
        FunctionExecutionDelegate next)
    {
        var startTimestamp = Stopwatch.GetTimestamp();

        var functionName =
            context.FunctionDefinition.Name;

        var tags = new TagList
        {
            { "function.name", functionName }
        };

        Meters.ActiveFunctions.Add(1, tags);

        logger.LogDebug(
            "Started executing function {FunctionName}.",
            functionName);

        try
        {
            await next(context);
        }
        finally
        {
            var elapsed =
                Stopwatch.GetElapsedTime(startTimestamp);

            logger.LogDebug(
                "Finished executing function {FunctionName} in {ElapsedMilliseconds} ms.",
                functionName,
                elapsed.TotalMilliseconds);

            try
            {
                Meters.FunctionCount.Add(1, tags);

                Meters.FunctionDuration.Record(
                    elapsed.TotalMilliseconds,
                    tags);
            }
            catch (Exception exception)
            {
                logger.LogDebug(
                    exception,
                    "Failed to record metrics for function {FunctionName}.",
                    functionName);
            }
            finally
            {
                Meters.ActiveFunctions.Add(-1, tags);
            }
        }
    }
}
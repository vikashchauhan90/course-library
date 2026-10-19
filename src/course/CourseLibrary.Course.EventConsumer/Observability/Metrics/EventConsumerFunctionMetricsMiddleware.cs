using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using CourseLibrary.Course.EventConsumer.Observability;

namespace CourseLibrary.Course.EventConsumer.Observability.Metrics;

public sealed class EventConsumerFunctionMetricsMiddleware(
    ILogger<EventConsumerFunctionMetricsMiddleware> logger)
    : IFunctionsWorkerMiddleware
{
    public async Task Invoke(
        FunctionContext context,
        FunctionExecutionDelegate next)
    {
        var startedAt = Stopwatch.GetTimestamp();
        var functionName = context.FunctionDefinition.Name;
        var outcome = "success";

        CourseEventConsumerMeter.Shared.RecordFunctionStarted(functionName);
        logger.FunctionExecutionStarted(functionName);

        try
        {
            await next(context);
        }
        catch (OperationCanceledException)
        {
            outcome = "cancelled";
            throw;
        }
        catch
        {
            outcome = "failure";
            throw;
        }
        finally
        {
            var duration = Stopwatch.GetElapsedTime(startedAt);
            CourseEventConsumerMeter.Shared.RecordFunctionCompleted(
                functionName,
                duration.TotalMilliseconds,
                outcome);
            logger.FunctionExecutionCompleted(
                functionName,
                outcome,
                duration.TotalMilliseconds);
        }
    }
}

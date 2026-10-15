using CourseLibrary.Course.EventConsumer.Observability;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CourseLibrary.Course.EventConsumer.Configuration.Observability.Metrics;

internal sealed class EventConsumerFunctionMetricsMiddleware(
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

        try
        {
            await next(context);
        }
        catch (OperationCanceledException)
        {
            outcome = "cancelled";
            throw;
        }
        catch (Exception exception)
        {
            outcome = "failure";
            logger.LogError(exception, "Function {FunctionName} failed.", functionName);
            throw;
        }
        finally
        {
            CourseEventConsumerMeter.Shared.RecordFunctionCompleted(
                functionName,
                Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds,
                outcome);
        }
    }
}

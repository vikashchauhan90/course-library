using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace CourseLibrary.Course.Application.Observability;

internal static class CourseApplicationOperation
{
    public static async Task<TResult> ExecuteAsync<TResult>(
        ILogger logger,
        string operation,
        Func<Task<TResult>> action)
    {
        using var activity = CourseApplicationActivitySource.Shared.StartActivity(operation);
        activity?.SetTag(CourseApplicationActivityTagConstants.Operation, operation);

        var stopwatch = Stopwatch.StartNew();
        logger.OperationStarted(operation);

        try
        {
            var result = await action();
            stopwatch.Stop();

            activity?.SetTag(CourseApplicationActivityTagConstants.OperationSuccess, true);
            activity?.SetTag(CourseApplicationActivityTagConstants.DurationMilliseconds, stopwatch.Elapsed.TotalMilliseconds);
            activity?.SetStatus(ActivityStatusCode.Ok);
            CourseApplicationMeter.Shared.RecordOperation(operation, "success", stopwatch.Elapsed.TotalMilliseconds);
            logger.OperationCompleted(operation, stopwatch.Elapsed.TotalMilliseconds);

            return result;
        }
        catch (OperationCanceledException)
        {
            stopwatch.Stop();

            activity?.SetTag(CourseApplicationActivityTagConstants.OperationCancelled, true);
            activity?.SetTag(CourseApplicationActivityTagConstants.DurationMilliseconds, stopwatch.Elapsed.TotalMilliseconds);
            activity?.SetStatus(ActivityStatusCode.Error, "Operation was cancelled.");
            CourseApplicationMeter.Shared.RecordFailure(operation, "cancelled");
            CourseApplicationMeter.Shared.RecordOperation(operation, "cancelled", stopwatch.Elapsed.TotalMilliseconds);
            logger.OperationCancelled(operation, stopwatch.Elapsed.TotalMilliseconds);
            throw;
        }
        catch (Exception exception)
        {
            stopwatch.Stop();

            activity?.SetTag(CourseApplicationActivityTagConstants.ErrorType, exception.GetType().Name);
            activity?.SetTag(CourseApplicationActivityTagConstants.DurationMilliseconds, stopwatch.Elapsed.TotalMilliseconds);
            activity?.SetStatus(ActivityStatusCode.Error, exception.Message);
            CourseApplicationMeter.Shared.RecordFailure(operation, "error");
            CourseApplicationMeter.Shared.RecordOperation(operation, "error", stopwatch.Elapsed.TotalMilliseconds);
            logger.OperationFailed(exception, operation, stopwatch.Elapsed.TotalMilliseconds);
            throw;
        }
    }
}

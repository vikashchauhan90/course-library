using System.Diagnostics;
using CourseLibrary.Core.Domain.Exceptions;
using CourseLibrary.Core.Results;
using CourseLibrary.Course.Application.Operations;
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

            if (result is Result { IsFailure: true } failure)
            {
                activity?.SetTag(CourseApplicationActivityTagConstants.OperationSuccess, false);
                activity?.SetTag(CourseApplicationActivityTagConstants.ErrorType, failure.Error?.Type.ToString());
                activity?.SetTag(CourseApplicationActivityTagConstants.DurationMilliseconds, stopwatch.Elapsed.TotalMilliseconds);
                activity?.SetStatus(ActivityStatusCode.Error, failure.Error?.Message);
                CourseApplicationMeter.Shared.RecordFailure(operation, "error");
                CourseApplicationMeter.Shared.RecordOperation(operation, "error", stopwatch.Elapsed.TotalMilliseconds);
                logger.OperationRejected(operation, failure.Error?.Code ?? "unknown", stopwatch.Elapsed.TotalMilliseconds);
                return result;
            }

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

    public static Task<Result<TResult>> ExecuteResultAsync<TResult>(
        ILogger logger,
        string operation,
        Func<Task<TResult>> action)
        => ExecuteAsync(
            logger,
            operation,
            async () =>
            {
                try
                {
                    return Result<TResult>.Success(await action());
                }
                catch (DomainException exception)
                {
                    return Result<TResult>.Failure(DomainExceptionErrorMapper.ToError(exception));
                }
            });

    public static Task<Result> ExecuteResultAsync(
        ILogger logger,
        string operation,
        Func<Task> action)
        => ExecuteAsync(
            logger,
            operation,
            async () =>
            {
                try
                {
                    await action();
                    return Result.Success();
                }
                catch (DomainException exception)
                {
                    return Result.Failure(DomainExceptionErrorMapper.ToError(exception));
                }
            });
}

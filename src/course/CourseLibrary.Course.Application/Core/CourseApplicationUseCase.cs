using System.Diagnostics;
using CourseLibrary.Core.Domain.Exceptions;
using CourseLibrary.Core.Results;
using CourseLibrary.Course.Application.Observability;
using Microsoft.Extensions.Logging;

namespace CourseLibrary.Course.Application.Core;

internal static class CourseApplicationUseCase
{
    public static async Task<TResult> ExecuteAsync<TResult>(
        ILogger logger,
        string useCase,
        Func<Task<TResult>> action)
    {
        using var activity = CourseApplicationActivitySource.Shared.StartActivity(useCase);
        activity?.SetTag(CourseApplicationActivityTagConstants.UseCase, useCase);

        var stopwatch = Stopwatch.StartNew();
        logger.UseCaseStarted(useCase);

        try
        {
            var result = await action();
            stopwatch.Stop();

            if (result is Result { IsFailure: true } failure)
            {
                activity?.SetTag(CourseApplicationActivityTagConstants.UseCaseSuccess, false);
                activity?.SetTag(CourseApplicationActivityTagConstants.ErrorType, failure.Error?.Type.ToString());
                activity?.SetTag(CourseApplicationActivityTagConstants.DurationMilliseconds, stopwatch.Elapsed.TotalMilliseconds);
                activity?.SetStatus(ActivityStatusCode.Error, failure.Error?.Message);
                CourseApplicationMeter.Shared.RecordFailure(useCase, "error");
                CourseApplicationMeter.Shared.RecordUseCase(useCase, "error", stopwatch.Elapsed.TotalMilliseconds);
                logger.UseCaseRejected(useCase, failure.Error?.Code ?? "unknown", stopwatch.Elapsed.TotalMilliseconds);
                return result;
            }

            activity?.SetTag(CourseApplicationActivityTagConstants.UseCaseSuccess, true);
            activity?.SetTag(CourseApplicationActivityTagConstants.DurationMilliseconds, stopwatch.Elapsed.TotalMilliseconds);
            activity?.SetStatus(ActivityStatusCode.Ok);
            CourseApplicationMeter.Shared.RecordUseCase(useCase, "success", stopwatch.Elapsed.TotalMilliseconds);
            logger.UseCaseCompleted(useCase, stopwatch.Elapsed.TotalMilliseconds);

            return result;
        }
        catch (OperationCanceledException)
        {
            stopwatch.Stop();

            activity?.SetTag(CourseApplicationActivityTagConstants.UseCaseCancelled, true);
            activity?.SetTag(CourseApplicationActivityTagConstants.DurationMilliseconds, stopwatch.Elapsed.TotalMilliseconds);
            activity?.SetStatus(ActivityStatusCode.Error, "Use case was cancelled.");
            CourseApplicationMeter.Shared.RecordFailure(useCase, "cancelled");
            CourseApplicationMeter.Shared.RecordUseCase(useCase, "cancelled", stopwatch.Elapsed.TotalMilliseconds);
            logger.UseCaseCancelled(useCase, stopwatch.Elapsed.TotalMilliseconds);
            throw;
        }
        catch (Exception exception)
        {
            stopwatch.Stop();

            activity?.SetTag(CourseApplicationActivityTagConstants.ErrorType, exception.GetType().Name);
            activity?.SetTag(CourseApplicationActivityTagConstants.DurationMilliseconds, stopwatch.Elapsed.TotalMilliseconds);
            activity?.SetStatus(ActivityStatusCode.Error, exception.Message);
            CourseApplicationMeter.Shared.RecordFailure(useCase, "error");
            CourseApplicationMeter.Shared.RecordUseCase(useCase, "error", stopwatch.Elapsed.TotalMilliseconds);
            logger.UseCaseFailed(exception, useCase, stopwatch.Elapsed.TotalMilliseconds);
            throw;
        }
    }

    public static Task<Result<TResult>> ExecuteResultAsync<TResult>(
        ILogger logger,
        string useCase,
        Func<Task<TResult>> action)
        => ExecuteAsync(
            logger,
            useCase,
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
        string useCase,
        Func<Task> action)
        => ExecuteAsync(
            logger,
            useCase,
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

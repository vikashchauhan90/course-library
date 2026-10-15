using System.Diagnostics;

namespace CourseLibrary.Course.Persistence.Observability;

internal static class CourseRepositoryObservability
{
    private const string DbSystem = "cosmosdb";
    private static readonly CourseActivitySource ActivitySource = CourseActivitySource.Shared;
    private static readonly CourseMeter Meter = CourseMeter.Shared;

    public static async Task<TResult> ExecuteAsync<TResult>(
        string repository,
        string operation,
        Func<Task<TResult>> execute)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(repository);
        ArgumentException.ThrowIfNullOrWhiteSpace(operation);
        ArgumentNullException.ThrowIfNull(execute);

        using var activity = ActivitySource.StartActivity(
            $"{repository}.{operation}",
            ActivityKind.Internal);
        var startedAt = Stopwatch.GetTimestamp();
        SetOperationTags(activity, repository, operation);

        try
        {
            var result = await execute();
            var outcome = result is null || result is false
                ? "not_found"
                : "success";
            activity?.SetTag("outcome", outcome);
            activity?.SetTag("db.operation.success", outcome == "success");
            activity?.SetStatus(ActivityStatusCode.Ok);

            Meter.RecordOperation(
                repository,
                operation,
                outcome,
                Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds);

            return result;
        }
        catch (OperationCanceledException exception)
        {
            RecordFailure(activity, repository, operation, startedAt, exception, "cancelled");
            throw;
        }
        catch (Exception exception)
        {
            RecordFailure(
                activity,
                repository,
                operation,
                startedAt,
                exception,
                "failure");
            throw;
        }
    }

    public static async Task ExecuteAsync(
        string repository,
        string operation,
        Func<Task> execute)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(repository);
        ArgumentException.ThrowIfNullOrWhiteSpace(operation);
        ArgumentNullException.ThrowIfNull(execute);

        using var activity = ActivitySource.StartActivity(
            $"{repository}.{operation}",
            ActivityKind.Internal);
        var startedAt = Stopwatch.GetTimestamp();
        SetOperationTags(activity, repository, operation);

        try
        {
            await execute();
            activity?.SetTag("outcome", "success");
            activity?.SetTag("db.operation.success", true);
            activity?.SetStatus(ActivityStatusCode.Ok);
            Meter.RecordOperation(
                repository,
                operation,
                "success",
                Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds);
        }
        catch (OperationCanceledException exception)
        {
            RecordFailure(activity, repository, operation, startedAt, exception, "cancelled");
            throw;
        }
        catch (Exception exception)
        {
            RecordFailure(
                activity,
                repository,
                operation,
                startedAt,
                exception,
                "failure");
            throw;
        }
    }

    private static void SetOperationTags(
        Activity? activity,
        string repository,
        string operation)
    {
        activity?.SetTag("db.system", DbSystem);
        activity?.SetTag("db.operation.name", operation);
        activity?.SetTag("repository.name", repository);
        activity?.SetTag("repository.operation", operation);
    }

    private static void RecordFailure(
        Activity? activity,
        string repository,
        string operation,
        long startedAt,
        Exception exception,
        string outcome)
    {
        activity?.SetTag("outcome", outcome);
        activity?.SetTag("error.type", exception.GetType().Name);
        activity?.SetTag("db.operation.cancelled", outcome == "cancelled");
        activity?.SetStatus(ActivityStatusCode.Error, exception.Message);

        Meter.RecordOperation(
            repository,
            operation,
            outcome,
            Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds,
            exception.GetType().Name);
    }
}

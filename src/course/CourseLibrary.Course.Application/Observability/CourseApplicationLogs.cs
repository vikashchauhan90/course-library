using Microsoft.Extensions.Logging;

namespace CourseLibrary.Course.Application.Observability;

internal static partial class CourseApplicationLogs
{
    [LoggerMessage(7001, LogLevel.Debug, "Application use case {UseCase} started.", EventName = nameof(UseCaseStarted))]
    public static partial void UseCaseStarted(this ILogger logger, string useCase);

    [LoggerMessage(7002, LogLevel.Debug, "Application use case {UseCase} completed in {ElapsedMilliseconds} ms.", EventName = nameof(UseCaseCompleted))]
    public static partial void UseCaseCompleted(this ILogger logger, string useCase, double elapsedMilliseconds);

    [LoggerMessage(7003, LogLevel.Warning, "Application use case {UseCase} was cancelled after {ElapsedMilliseconds} ms.", EventName = nameof(UseCaseCancelled))]
    public static partial void UseCaseCancelled(this ILogger logger, string useCase, double elapsedMilliseconds);

    [LoggerMessage(7004, LogLevel.Error, "Application use case {UseCase} failed after {ElapsedMilliseconds} ms.", EventName = nameof(UseCaseFailed))]
    public static partial void UseCaseFailed(
        this ILogger logger,
        Exception exception,
        string useCase,
        double elapsedMilliseconds);

    [LoggerMessage(7005, LogLevel.Warning, "Application use case {UseCase} was rejected with error {ErrorCode} after {ElapsedMilliseconds} ms.", EventName = nameof(UseCaseRejected))]
    public static partial void UseCaseRejected(
        this ILogger logger,
        string useCase,
        string errorCode,
        double elapsedMilliseconds);
}

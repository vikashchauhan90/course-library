using MediatorForge.Abstractions;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CourseLibrary.Application.Behaviors;

public sealed class PerformanceBehavior<TRequest, TResponse>(
    ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : notnull
{

    public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var requestStart = Stopwatch.GetTimestamp();
        var response = await next();
        var elapsed = Stopwatch.GetElapsedTime(requestStart);
        logger.LogInformation(
            "Request {RequestName} executed in {ElapsedMs}ms",
            typeof(TRequest).Name,
            elapsed.TotalMilliseconds);
        return response;
    }
}

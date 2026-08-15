using MediatorForge.Abstractions;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CourseLibrary.Application.Behaviors;

public sealed class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;

    public PerformanceBehavior(ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        var response = await next();
        sw.Stop();
        _logger.LogInformation("Request {RequestName} executed in {ElapsedMs}ms", typeof(TRequest).Name, sw.Elapsed.TotalMilliseconds);
        return response;
    }
}

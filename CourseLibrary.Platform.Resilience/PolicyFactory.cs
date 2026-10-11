using CourseLibrary.Platform.Observability.Traces;
using CourseLibrary.Platform.Resilience.Abstractions;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Registry;

namespace CourseLibrary.Platform.Resilience;

internal sealed class PolicyFactory(
    ResiliencePipelineProvider<string> provider,
    ILogger<PolicyFactory> logger) : IPolicyFactory
{
    private static readonly PlatformActivitySource ActivitySources = new();
    public ResiliencePipeline Get(string policyName)
    {
        return provider.GetPipeline(policyName);
    }

    public ValueTask ExecuteAsync(
        string policyName,
        Func<CancellationToken, ValueTask> operation,
        CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySources.StartActivity(
           "PolicyFactory.ExecuteAsync");
        activity?.SetTag("policyName", policyName);
        activity?.SetTag("operationType", "void");
        activity?.SetTag("operationMethod", operation.Method.Name);
        activity?.SetTag("startTime", DateTime.UtcNow.ToString("o"));
        var pipeline = provider.GetPipeline(policyName);

        return pipeline.ExecuteAsync(
            operation,
            cancellationToken);
    }

    public ValueTask<T> ExecuteAsync<T>(
        string policyName,
        Func<CancellationToken, ValueTask<T>> operation,
        CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySources.StartActivity(
            "PolicyFactory.ExecuteAsync<T>");
        activity?.SetTag("policyName", policyName);
        activity?.SetTag("operationType", typeof(T).Name);
        activity?.SetTag("operationMethod", operation.Method.Name);
        activity?.SetTag("startTime", DateTime.UtcNow.ToString("o"));
        var pipeline = provider.GetPipeline(policyName);

        return pipeline.ExecuteAsync(
            operation,
            cancellationToken);
    }
}

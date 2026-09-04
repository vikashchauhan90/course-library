using CourseLibrary.Infrastructure.Observability.Traces;
using Polly;
using Polly.Registry;
using System.Diagnostics;

namespace CourseLibrary.Infrastructure.Resilience;

public sealed class PolicyFactory(
    ResiliencePipelineProvider<string> provider)
{
    public ResiliencePipeline Get(string policyName)
    {
        return provider.GetPipeline(policyName);
    }

    public ValueTask ExecuteAsync(
        string policyName,
        Func<CancellationToken, ValueTask> operation,
        CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySources.Infrastructure.StartActivity(
           "PolicyFactory.ExecuteAsync",
           ActivityKind.Internal);
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
        using var activity = ActivitySources.Infrastructure.StartActivity(
            "PolicyFactory.ExecuteAsync<T>",
            ActivityKind.Internal);
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

using Polly;
using Polly.Registry;

namespace CourseLibrary.Idp.Infrastructure.Resilience;

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
        var pipeline = provider.GetPipeline(policyName);

        return pipeline.ExecuteAsync(
            operation,
            cancellationToken);
    }
}

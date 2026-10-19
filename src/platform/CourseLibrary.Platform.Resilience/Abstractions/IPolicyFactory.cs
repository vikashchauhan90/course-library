using Polly;

namespace CourseLibrary.Platform.Resilience.Abstractions;

public interface IPolicyFactory
{
    ValueTask ExecuteAsync(string policyName, Func<CancellationToken, ValueTask> operation, CancellationToken cancellationToken = default);
    ValueTask<T> ExecuteAsync<T>(string policyName, Func<CancellationToken, ValueTask<T>> operation, CancellationToken cancellationToken = default);
    ResiliencePipeline Get(string policyName);
}
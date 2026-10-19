namespace CourseLibrary.Platform.Resilience.Distributed.Abstractions;

/// <summary>
/// Executes operations through a Polly retry, timeout, and circuit-breaker pipeline
/// coordinated with shared circuit state.
/// </summary>
public interface IDistributedResilience
{
    /// <summary>Executes an operation that does not return a result.</summary>
    ValueTask ExecuteAsync(
        DistributedCircuitKey key,
        Func<CancellationToken, ValueTask> operation,
        CancellationToken cancellationToken = default);

    /// <summary>Executes an operation that returns a result.</summary>
    ValueTask<TResult> ExecuteAsync<TResult>(
        DistributedCircuitKey key,
        Func<CancellationToken, ValueTask<TResult>> operation,
        CancellationToken cancellationToken = default);
}

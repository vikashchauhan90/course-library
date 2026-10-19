namespace CourseLibrary.Platform.Resilience.Distributed.Abstractions;

public interface IDistributedResilienceStore
{
    /// <summary>Reads the shared circuit state and whether a half-open probe is currently leased.</summary>
    ValueTask<DistributedCircuitStatus> GetStateAsync(
        DistributedCircuitKey key,
        CancellationToken cancellationToken = default);

    /// <summary>Opens or extends the shared circuit for the specified duration.</summary>
    ValueTask OpenAsync(
        DistributedCircuitKey key,
        TimeSpan duration,
        CancellationToken cancellationToken = default);

    /// <summary>Attempts to acquire the exclusive probe lease after the open interval has elapsed.</summary>
    ValueTask<DistributedProbeLease?> TryAcquireProbeAsync(
        DistributedCircuitKey key,
        TimeSpan leaseDuration,
        CancellationToken cancellationToken = default);

    /// <summary>Closes the circuit on success or reopens it on failure, if the lease is still owned.</summary>
    ValueTask<bool> CompleteProbeAsync(
        DistributedCircuitKey key,
        DistributedProbeLease lease,
        bool succeeded,
        TimeSpan openDurationOnFailure,
        CancellationToken cancellationToken = default);
}

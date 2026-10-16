namespace CourseLibrary.Platform.Resilience.Distributed.Abstractions;

public sealed class DistributedCircuitOpenException(
    DistributedCircuitKey key,
    DateTimeOffset? openUntil)
    : InvalidOperationException(
        $"Distributed circuit for '{key.Dependency}/{key.Operation}' is open.")
{
    public DistributedCircuitKey Key { get; } = key;

    public DateTimeOffset? OpenUntil { get; } = openUntil;
}

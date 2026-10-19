namespace CourseLibrary.Platform.Resilience.Distributed.Abstractions;

public sealed record DistributedCircuitStatus(
    DistributedCircuitState State,
    DateTimeOffset? OpenUntil,
    bool ProbeInProgress);

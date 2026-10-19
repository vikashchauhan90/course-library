namespace CourseLibrary.Platform.Resilience.Distributed.Observability;

internal static class DistributedResilienceActivityTagConstants
{
    public const string Dependency = "resilience.distributed.dependency";
    public const string Operation = "resilience.distributed.operation";
    public const string OperationType = "resilience.operation";
    public const string Outcome = "resilience.outcome";
    public const string State = "resilience.distributed.state";
    public const string ProbeAcquired = "resilience.distributed.probe_acquired";
    public const string ProbeSucceeded = "resilience.distributed.probe_succeeded";
    public const string LeaseCompleted = "resilience.distributed.lease_completed";
}

namespace CourseLibrary.Platform.Resilience.Distributed.Abstractions;

public enum DistributedCircuitState
{
    Closed,
    Open,
    HalfOpen
}

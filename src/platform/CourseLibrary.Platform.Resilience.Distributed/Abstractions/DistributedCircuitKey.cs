namespace CourseLibrary.Platform.Resilience.Distributed.Abstractions;

public readonly record struct DistributedCircuitKey
{
    public DistributedCircuitKey(string dependency, string operation)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(dependency);
        ArgumentException.ThrowIfNullOrWhiteSpace(operation);

        Dependency = dependency;
        Operation = operation;
    }

    public string Dependency { get; }

    public string Operation { get; }
}

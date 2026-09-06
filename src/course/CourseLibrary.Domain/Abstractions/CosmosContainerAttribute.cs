namespace CourseLibrary.Domain.Abstractions;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class CosmosContainerAttribute : Attribute
{
    public CosmosContainerAttribute(string containerName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(containerName);

        ContainerName = containerName;
    }

    public string ContainerName { get; }
}
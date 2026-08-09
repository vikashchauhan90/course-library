namespace CourseLibrary.Domain.Entities;

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
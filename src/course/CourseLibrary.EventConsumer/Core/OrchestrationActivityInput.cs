namespace CourseLibrary.EventConsumer.Core;

internal class OrchestrationActivityInput
{
    public byte[] Event { get; init; } = default!;
    public string? ParentActivityId { get; init; }
}

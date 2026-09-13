namespace CourseLibrary.EventConsumer.Core;

internal class OrchestrationActivityInput<T>
{
    public T Event { get; init; } = default!;
    public string MessageId { get; init; } = string.Empty;
    public string? ParentActivityId { get; init; }
}

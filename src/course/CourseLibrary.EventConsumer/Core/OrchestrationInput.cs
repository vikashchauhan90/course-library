namespace CourseLibrary.EventConsumer.Core;

internal class OrchestrationInput
{
    public T Event { get; init; } = default!;
    public string MessageId { get; init; } = string.Empty;
    public string? ParentTraceParent { get; init; }
    public string? ParentTraceState { get; init; }
}

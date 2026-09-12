namespace CourseLibrary.EventConsumer.Core;

internal class OrchestrationInput<T>
{
    public T Event { get; init; } = default!;
    public string? ParentTraceParent { get; init; }
    public string? ParentTraceState { get; init; }
}

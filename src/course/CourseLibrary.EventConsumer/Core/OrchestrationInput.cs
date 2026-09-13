namespace CourseLibrary.EventConsumer.Core;

internal class OrchestrationInput
{
    public byte[] Event { get; init; } = default!;
    public string? ParentTraceParent { get; init; }
    public string? ParentTraceState { get; init; }
}

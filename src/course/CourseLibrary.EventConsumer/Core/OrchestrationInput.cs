using System.Diagnostics;


namespace CourseLibrary.EventConsumer.Core;

internal class OrchestrationInput<T>
{
    public T Event { get; init; } = default!;
    public ActivityContext? ParentContext { get; init; }
}

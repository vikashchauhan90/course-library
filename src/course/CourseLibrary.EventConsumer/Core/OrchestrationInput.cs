using CourseLibrary.Infrastructure.Configuration.Messaging;

namespace CourseLibrary.EventConsumer.Core;

internal class OrchestrationInput<T>
{
    public T Event { get; init; } = default!;
    public string MessageId { get; init; } = string.Empty;
    public EventMetadata Metadata { get; init; } = default!;
}

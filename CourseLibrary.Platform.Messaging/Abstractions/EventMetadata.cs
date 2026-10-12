
using System.Collections.Immutable;

namespace CourseLibrary.Platform.Messaging.Abstractions;

public sealed record EventMetadata
{
    public string? TraceParent { get; init; }
    public string? TraceState { get; init; }
    public ImmutableDictionary<string, string> Properties { get; init; }
        = ImmutableDictionary<string, string>.Empty;
}

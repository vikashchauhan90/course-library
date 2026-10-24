namespace CourseLibrary.Platform.Observability;

public sealed class ObservabilityOptions
{
    /// <summary>Prefix applied to user-defined log attribute keys. Defaults to "course.library.platform".</summary>
    public string? LogKeyPrefix { get; set; }
}
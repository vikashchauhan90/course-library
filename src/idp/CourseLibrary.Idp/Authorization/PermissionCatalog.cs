namespace CourseLibrary.Idp.Authorization;

internal static class PermissionCatalog
{
    public const string ClaimType = "permission";
    public const string AdministratorRole = "Administrator";

    public static IReadOnlyList<PermissionDefinition> Definitions { get; } =
    [
        new("course.read", "Course read"),
        new("course.write", "Course write"),
        new("course.update", "Course update"),
        new("course.delete", "Course delete"),
        new("course.all", "All course actions"),
        new("comment.read", "Comment read"),
        new("comment.write", "Comment write"),
        new("comment.update", "Comment update"),
        new("comment.delete", "Comment delete"),
        new("comment.all", "All comment actions"),
        new("discussion.read", "Discussion read"),
        new("discussion.write", "Discussion write"),
        new("discussion.update", "Discussion update"),
        new("discussion.delete", "Discussion delete"),
        new("discussion.all", "All discussion actions")
    ];

    public static bool TryNormalize(
        string? value,
        out string normalized)
    {
        var candidate = value?.Trim().ToLowerInvariant() ?? string.Empty;
        normalized = candidate;
        return Definitions.Any(definition =>
            definition.Value.Equals(
                candidate,
                StringComparison.OrdinalIgnoreCase));
    }
}

internal sealed record PermissionDefinition(string Value, string DisplayName);
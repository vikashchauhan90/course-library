namespace CourseLibrary.Idp.Domain.Authorization;

public static class PermissionCatalog
{
    public const string ClaimType = "permission";
    public const string AdministratorRole = "Administrator";

    public static IReadOnlyList<PermissionDefinition> Definitions { get; } =
    [
        new("course.read", "Course read", "course", "read"),
        new("course.write", "Course write", "course", "write"),
        new("course.update", "Course update", "course", "update"),
        new("course.delete", "Course delete", "course", "delete"),
        new("course.all", "All course actions", "course", "all"),
        new("comment.read", "Comment read", "comment", "read"),
        new("comment.write", "Comment write", "comment", "write"),
        new("comment.update", "Comment update", "comment", "update"),
        new("comment.delete", "Comment delete", "comment", "delete"),
        new("comment.all", "All comment actions", "comment", "all"),
        new("discussion.read", "Discussion read", "discussion", "read"),
        new("discussion.write", "Discussion write", "discussion", "write"),
        new("discussion.update", "Discussion update", "discussion", "update"),
        new("discussion.delete", "Discussion delete", "discussion", "delete"),
        new("discussion.all", "All discussion actions", "discussion", "all")
    ];

    public static bool TryNormalize(
        string? value,
        out string normalized)
    {
        var candidate = value?.Trim().ToLowerInvariant() ?? string.Empty;
        normalized = candidate;
        return Definitions.Any(definition =>
            definition.Code.Equals(candidate, StringComparison.OrdinalIgnoreCase));
    }
}

public sealed record PermissionDefinition(
    string Code,
    string DisplayName,
    string Resource,
    string Action);
namespace CourseLibrary.Course.Persistence.Core;

internal static class SupportedContainers
{
    public const string Courses = "courses";
    public const string Discussions = "discussions";
    public const string CourseAudits = "course-audits";
    public const string Comments = "comments";

    public static readonly IReadOnlyList<string> All = new List<string>
    {
        Courses,
        Discussions,
        CourseAudits,
        Comments
    };

    public static bool IsSupported(string containerName)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(containerName);
        return All.Contains(containerName);
    }

    public static void ValidateContainer(string containerName)
    {
        if (!IsSupported(containerName))
        {
            throw new NotSupportedException($"Container '{containerName}' is not supported.");
        }
    }
}

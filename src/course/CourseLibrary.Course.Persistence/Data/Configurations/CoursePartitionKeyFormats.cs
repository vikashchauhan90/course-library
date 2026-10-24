using System.Globalization;

namespace CourseLibrary.Course.Persistence.Data.Configurations;

public static class CoursePartitionKeyFormats
{
    public const string AuthorPrefix = "#AUTHOR#";
    public const string CoursePrefix = "#COURSE#";

    public const string AuthorFormat = AuthorPrefix + "{0}";
    public const string CourseFormat = CoursePrefix + "{0}";

    public static string ForAuthor(string authorId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(authorId);
        return string.Format(CultureInfo.InvariantCulture, AuthorFormat, authorId);
    }

    public static string ForCourse(string courseId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(courseId);
        return string.Format(CultureInfo.InvariantCulture, CourseFormat, courseId);
    }
}

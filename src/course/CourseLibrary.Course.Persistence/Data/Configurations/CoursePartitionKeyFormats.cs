namespace CourseLibrary.Course.Persistence.Data.Configurations;

internal static class CoursePartitionKeyFormats
{
    internal const string AuthorPrefix = "#AUTHOR#";
    internal const string CoursePrefix = "#COURSE#";

    internal const string AuthorFormat = AuthorPrefix + "{0}";
    internal const string CourseFormat = CoursePrefix + "{0}";
}

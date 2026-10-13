using CourseLibrary.Course.Domain.ValueObjects;

namespace CourseLibrary.Course.Application.Operations;

internal static class OperationInput
{
    public static CourseId CourseId(string value, string parameterName = "courseId")
        => new(ParseGuid(value, parameterName));

    public static CommentId CommentId(string value, string parameterName = "commentId")
        => new(ParseGuid(value, parameterName));

    public static DiscussionId DiscussionId(string value, string parameterName = "discussionId")
        => new(ParseGuid(value, parameterName));

    public static void Required(string? value, string parameterName)
        => ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);

    private static string ParseGuid(string value, string parameterName)
    {
        Required(value, parameterName);
        if (!Guid.TryParse(value, out var id))
        {
            throw new ArgumentException("The value must be a valid identifier.", parameterName);
        }

        return id.ToString();
    }
}

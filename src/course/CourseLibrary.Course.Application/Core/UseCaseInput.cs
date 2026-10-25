using CourseLibrary.Platform.Exceptions;

namespace CourseLibrary.Course.Application.Core;

internal static class UseCaseInput
{
    public static string CourseId(string value, string parameterName = "courseId")
        => ParseGuid(value, parameterName);

    public static string CommentId(string value, string parameterName = "commentId")
        => ParseGuid(value, parameterName);

    public static string DiscussionId(string value, string parameterName = "discussionId")
        => ParseGuid(value, parameterName);

    public static void Required(string? value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new BadRequestException($"'{parameterName}' is required.");
        }
    }

    private static string ParseGuid(string value, string parameterName)
    {
        Required(value, parameterName);
        if (!Guid.TryParse(value, out var id))
        {
            throw new BadRequestException($"'{parameterName}' must be a valid identifier.");
        }

        return id.ToString();
    }
}

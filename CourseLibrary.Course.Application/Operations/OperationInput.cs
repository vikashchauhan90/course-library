using CourseLibrary.Course.Domain.ValueObjects;
using CourseLibrary.Core.Domain.Exceptions;

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

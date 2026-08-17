using CourseLibrary.Application.Operations.Discussions.Delete;

namespace CourseLibrary.Api.Endpoints.Discussions.DeleteDiscussion;

public static class DeleteDiscussionMapper
{
    public static DeleteDiscussionCommand ToCommand(string discussionId, string courseId)
        => new(discussionId, courseId);
}

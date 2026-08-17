using CourseLibrary.Application.Operations.Discussions.Update;

namespace CourseLibrary.Api.Endpoints.Discussions.UpdateDiscussion;

public static class UpdateDiscussionMapper
{
    public static UpdateDiscussionCommand ToCommand(string discussionId, string courseId, UpdateDiscussionRequest request)
        => new(discussionId, courseId, request.Title, request.Description);
}

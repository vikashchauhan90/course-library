using CourseLibrary.Application.Operations.Discussions.Get;

namespace CourseLibrary.Api.Endpoints.Discussions.GetDiscussion;

public static class GetDiscussionMapper
{
    public static GetDiscussionQuery ToQuery(string discussionId, string courseId)
        => new(discussionId, courseId);
}

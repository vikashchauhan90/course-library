using CourseLibrary.Application.Operations.Discussions.Create;

namespace CourseLibrary.Api.Endpoints.Discussions.CreateDiscussion;

public static class CreateDiscussionMapper
{
    public static CreateDiscussionCommand ToCommand(CreateDiscussionRequest request)
        => new(request.CourseId, request.Title, request.Description);
}

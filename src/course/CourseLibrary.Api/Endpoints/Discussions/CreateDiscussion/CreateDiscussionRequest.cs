namespace CourseLibrary.Api.Endpoints.Discussions.CreateDiscussion;

public sealed record CreateDiscussionRequest(string CourseId, string Title, string Description);

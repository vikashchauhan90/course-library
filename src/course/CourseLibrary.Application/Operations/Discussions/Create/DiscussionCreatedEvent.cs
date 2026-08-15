namespace CourseLibrary.Application.Operations.Discussions.Create;

public sealed record DiscussionCreatedEvent(string DiscussionId, string CourseId, string Title, DateTime CreatedAt);

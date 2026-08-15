namespace CourseLibrary.Application.Operations.Discussions.Update;

public sealed record DiscussionUpdatedEvent(string DiscussionId, string CourseId, string Title, DateTime UpdatedAt);

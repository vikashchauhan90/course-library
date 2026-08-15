namespace CourseLibrary.Application.Operations.Discussions.Delete;

public sealed record DiscussionDeletedEvent(string DiscussionId, string CourseId);

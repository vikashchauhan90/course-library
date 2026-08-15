namespace CourseLibrary.Application.Operations.Authors.Update;

public sealed record AuthorUpdatedEvent(string AuthorId, string Name, DateTime UpdatedAt);

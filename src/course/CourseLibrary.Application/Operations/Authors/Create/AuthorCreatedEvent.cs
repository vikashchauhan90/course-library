namespace CourseLibrary.Application.Operations.Authors.Create;

public sealed record AuthorCreatedEvent(string AuthorId, string Name, DateTime CreatedAt);

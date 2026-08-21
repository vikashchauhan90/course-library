namespace CourseLibrary.Domain.Events;

public sealed record AuthorUpdatedEvent(string AuthorId, string Name, DateTime UpdatedAt);
namespace CourseLibrary.Domain.Events;

public sealed record CourseCreatedEvent(string CourseId, string AuthorId, string Title, DateTime CreatedAt);
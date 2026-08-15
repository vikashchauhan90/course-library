namespace CourseLibrary.Application.Operations.Courses.Create;

public sealed record CourseCreatedEvent(string CourseId, string AuthorId, string Title, DateTime CreatedAt);

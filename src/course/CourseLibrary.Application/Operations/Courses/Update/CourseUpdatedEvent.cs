namespace CourseLibrary.Application.Operations.Courses.Update;

public sealed record CourseUpdatedEvent(string CourseId, string AuthorId, string Title, DateTime UpdatedAt);

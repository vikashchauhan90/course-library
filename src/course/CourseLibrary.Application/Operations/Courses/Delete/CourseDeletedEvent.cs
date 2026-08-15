namespace CourseLibrary.Application.Operations.Courses.Delete;

public sealed record CourseDeletedEvent(string CourseId, string PartitionKey);

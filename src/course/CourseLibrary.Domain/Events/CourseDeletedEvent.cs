namespace CourseLibrary.Domain.Events;

public sealed record CourseDeletedEvent(string CourseId, string PartitionKey);
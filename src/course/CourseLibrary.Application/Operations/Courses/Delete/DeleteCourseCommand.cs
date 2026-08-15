using MediatorForge.Abstractions;

namespace CourseLibrary.Application.Operations.Courses.Delete;

public sealed record DeleteCourseCommand(string CourseId, string PartitionKey) : ICommand<bool>;

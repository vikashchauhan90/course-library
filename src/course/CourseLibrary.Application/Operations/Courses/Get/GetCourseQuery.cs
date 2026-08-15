using MediatorForge.Abstractions;

namespace CourseLibrary.Application.Operations.Courses.Get;

public sealed record GetCourseQuery(string CourseId, string PartitionKey) : IQuery<Domain.Entities.Course?>;

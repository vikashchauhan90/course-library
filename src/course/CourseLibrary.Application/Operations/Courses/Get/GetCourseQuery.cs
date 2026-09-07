using MediatorForge.Abstractions;
using CourseLibrary.Application.Operations.Courses;

namespace CourseLibrary.Application.Operations.Courses.Get;

public sealed record GetCourseQuery(string CourseId) : IQuery<CourseResponse?>;

using CourseLibrary.Models.Course;
using MediatorForge.Abstractions;

namespace CourseLibrary.Application.Operations.Courses.Get;

public sealed record GetCourseQuery(string CourseId) : IQuery<CourseResponse?>;

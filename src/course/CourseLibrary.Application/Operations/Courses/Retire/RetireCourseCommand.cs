using MediatorForge.Abstractions;

namespace CourseLibrary.Application.Operations.Courses.Retire;

public sealed record RetireCourseCommand(string CourseId, string AuthorId) : ICommand<CourseResponse?>;
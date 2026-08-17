using MediatorForge.Abstractions;
using CourseLibrary.Application.Operations.Courses;

namespace CourseLibrary.Application.Operations.Courses.Create;

public sealed record CreateCourseCommand(string Title, string Description, string AuthorId) : ICommand<CourseResponse>;

using MediatorForge.Abstractions;
using CourseLibrary.Application.Operations.Courses;

namespace CourseLibrary.Application.Operations.Courses.Update;

public sealed record UpdateCourseCommand(string Id, string Title, string Description, string AuthorId) : ICommand<CourseResponse>;

using MediatorForge.Abstractions;

namespace CourseLibrary.Application.Operations.Courses.Create;

public sealed record CreateCourseCommand(string Title, string Description, string AuthorId) : ICommand<Domain.Entities.Course>;

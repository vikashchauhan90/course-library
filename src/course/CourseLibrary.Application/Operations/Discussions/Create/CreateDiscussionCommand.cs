using MediatorForge.Abstractions;
using CourseLibrary.Models.Course;

namespace CourseLibrary.Application.Operations.Discussions.Create;

public sealed record CreateDiscussionCommand(string CourseId, string Title, string Description) : ICommand<DiscussionResponse>;

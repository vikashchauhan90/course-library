using MediatorForge.Abstractions;

namespace CourseLibrary.Application.Operations.Discussions.Create;

public sealed record CreateDiscussionCommand(string CourseId, string Title, string Description) : ICommand<CourseLibrary.Domain.Entities.Discussion>;

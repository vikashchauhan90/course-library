using MediatorForge.Abstractions;

namespace CourseLibrary.Application.Operations.Discussions.Update;

public sealed record UpdateDiscussionCommand(string Id, string CourseId, string Title, string Description) : ICommand<CourseLibrary.Domain.Entities.Discussion>;

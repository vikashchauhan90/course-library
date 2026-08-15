using MediatorForge.Abstractions;

namespace CourseLibrary.Application.Operations.Discussions.Delete;

public sealed record DeleteDiscussionCommand(string DiscussionId, string CourseId) : ICommand<bool>;

using MediatorForge.Abstractions;
using CourseLibrary.Application.Operations.Discussions;

namespace CourseLibrary.Application.Operations.Discussions.Create;

public sealed record CreateDiscussionCommand(string CourseId, string Title, string Description) : ICommand<DiscussionResponse>;

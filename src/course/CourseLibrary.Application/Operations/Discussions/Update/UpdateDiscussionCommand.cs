using MediatorForge.Abstractions;
using CourseLibrary.Application.Operations.Discussions;

namespace CourseLibrary.Application.Operations.Discussions.Update;

public sealed record UpdateDiscussionCommand(string Id, string CourseId, string Title, string Description) : ICommand<DiscussionResponse>;

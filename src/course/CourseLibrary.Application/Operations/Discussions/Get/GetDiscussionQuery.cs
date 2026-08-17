using MediatorForge.Abstractions;
using CourseLibrary.Application.Operations.Discussions;

namespace CourseLibrary.Application.Operations.Discussions.Get;

public sealed record GetDiscussionQuery(string DiscussionId, string CourseId) : IQuery<DiscussionResponse?>;

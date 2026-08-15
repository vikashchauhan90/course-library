using MediatorForge.Abstractions;

namespace CourseLibrary.Application.Operations.Discussions.Get;

public sealed record GetDiscussionQuery(string DiscussionId, string CourseId) : IQuery<CourseLibrary.Domain.Entities.Discussion?>;

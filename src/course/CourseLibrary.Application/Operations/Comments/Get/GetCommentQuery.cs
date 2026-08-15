using MediatorForge.Abstractions;

namespace CourseLibrary.Application.Operations.Comments.Get;

public sealed record GetCommentQuery(string CommentId, string CourseId) : IQuery<CourseLibrary.Domain.Entities.Comment?>;

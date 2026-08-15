using MediatorForge.Abstractions;

namespace CourseLibrary.Application.Operations.Comments.Create;

public sealed record CreateCommentCommand(string CourseId, string AuthorId, string Content, string? ParentCommentId) : ICommand<CourseLibrary.Domain.Entities.Comment>;

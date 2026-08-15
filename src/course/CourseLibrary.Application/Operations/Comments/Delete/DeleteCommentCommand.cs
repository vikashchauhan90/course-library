using MediatorForge.Abstractions;

namespace CourseLibrary.Application.Operations.Comments.Delete;

public sealed record DeleteCommentCommand(string CommentId, string CourseId) : ICommand<bool>;

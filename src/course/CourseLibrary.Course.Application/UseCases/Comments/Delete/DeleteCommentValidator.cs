using CourseLibrary.Course.Application.Core;
using FluentValidation;

namespace CourseLibrary.Course.Application.UseCases.Comments.Delete;

internal sealed record DeleteCommentRequest(string? CommentId, string? CourseId);

internal sealed class DeleteCommentValidator : AbstractValidator<DeleteCommentRequest>
{
    public DeleteCommentValidator()
    {
        RuleFor(request => request.CommentId).Identifier("CommentId");
        RuleFor(request => request.CourseId).Identifier("CourseId");
    }
}

using CourseLibrary.Course.Application.Core;
using FluentValidation;

namespace CourseLibrary.Course.Application.Operations.Comments.Update;

internal sealed record UpdateCommentRequest(string? CommentId, string? CourseId, string? Content);

internal sealed class UpdateCommentValidator : AbstractValidator<UpdateCommentRequest>
{
    public UpdateCommentValidator()
    {
        RuleFor(request => request.CommentId).Identifier("CommentId");
        RuleFor(request => request.CourseId).Identifier("CourseId");
        RuleFor(request => request.Content).RequiredText();
    }
}

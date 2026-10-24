using CourseLibrary.Course.Application.Core;
using FluentValidation;

namespace CourseLibrary.Course.Application.UseCases.Comments.Get;

internal sealed record GetCommentRequest(string? CommentId, string? CourseId);

internal sealed class GetCommentValidator : AbstractValidator<GetCommentRequest>
{
    public GetCommentValidator()
    {
        RuleFor(request => request.CommentId).Identifier("CommentId");
        RuleFor(request => request.CourseId).Identifier("CourseId");
    }
}

using CourseLibrary.Course.Application.Core;
using CourseLibrary.Course.Contracts.DTOs.Courses.Comments;
using FluentValidation;

namespace CourseLibrary.Course.Application.UseCases.Comments.Create;

internal sealed class CreateCommentValidator : AbstractValidator<CommentRequest>
{
    public CreateCommentValidator()
    {
        RuleFor(request => request.CourseId).Identifier("CourseId");
        RuleFor(request => request.AuthorId).RequiredText();
        RuleFor(request => request.Content).RequiredText();
        RuleFor(request => request.ParentCommentId).OptionalIdentifier("ParentCommentId");
    }
}

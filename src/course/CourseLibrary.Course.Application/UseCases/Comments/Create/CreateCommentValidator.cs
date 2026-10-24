using CourseLibrary.Course.Application.Core;
using FluentValidation;

namespace CourseLibrary.Course.Application.UseCases.Comments.Create;

internal sealed record CreateCommentUseCaseRequest(
    string? CourseId,
    string? AuthorId,
    string? Content,
    string? ParentCommentId);

internal sealed class CreateCommentValidator : AbstractValidator<CreateCommentUseCaseRequest>
{
    public CreateCommentValidator()
    {
        RuleFor(request => request.CourseId).Identifier("CourseId");
        RuleFor(request => request.AuthorId).RequiredText();
        RuleFor(request => request.Content).RequiredText();
        RuleFor(request => request.ParentCommentId).OptionalIdentifier("ParentCommentId");
    }
}

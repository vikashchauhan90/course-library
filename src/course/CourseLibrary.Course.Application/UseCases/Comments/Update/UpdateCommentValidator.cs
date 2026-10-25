using CourseLibrary.Course.Application.Core;
using CourseLibrary.Course.Contracts.DTOs.Courses.Comments;
using FluentValidation;

namespace CourseLibrary.Course.Application.UseCases.Comments.Update;

internal sealed class UpdateCommentValidator : AbstractValidator<UpdateCommentRequest>
{
    public UpdateCommentValidator()
    {
        RuleFor(request => request.Content).RequiredText();
    }
}

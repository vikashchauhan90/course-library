using CourseLibrary.Course.Application.Core;
using CourseLibrary.Course.Contracts.DTOs.Courses.Discussions;
using FluentValidation;

namespace CourseLibrary.Course.Application.UseCases.Discussions.Update;

internal sealed class UpdateDiscussionValidator : AbstractValidator<UpdateDiscussionRequest>
{
    public UpdateDiscussionValidator()
    {
        RuleFor(request => request.Title).RequiredText();
        RuleFor(request => request.Description).RequiredText();
    }
}

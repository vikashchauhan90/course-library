using FluentValidation;

namespace CourseLibrary.Application.Operations.Courses.Create;

public sealed class CreateCourseValidator : AbstractValidator<CreateCourseCommand>
{
    public CreateCourseValidator()
    {
        RuleFor(x => x.Title).NotEmpty().WithMessage("Title is required");
        RuleFor(x => x.AuthorId).NotEmpty().WithMessage("AuthorId is required");
        RuleFor(x => x.Description).NotEmpty().WithMessage("Description is required");
    }
}

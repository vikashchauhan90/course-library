using FluentValidation;

namespace CourseLibrary.Course.Application.Operations.Courses.Search;

internal sealed record SearchCoursesRequest(int PageSize);

internal sealed class SearchCoursesValidator : AbstractValidator<SearchCoursesRequest>
{
    public SearchCoursesValidator()
        => RuleFor(request => request.PageSize)
            .GreaterThan(0);
}

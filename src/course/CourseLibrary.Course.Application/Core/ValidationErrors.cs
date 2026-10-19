using CourseLibrary.Core.Domain.Exceptions;
using FluentValidation.Results;

namespace CourseLibrary.Course.Application.Core;

internal static class ValidationErrors
{
    public static void ThrowIfInvalid(ValidationResult validationResult)
    {
        if (validationResult.IsValid)
        {
            return;
        }

        throw new ValidationFailedException
        {
            Errors = validationResult.Errors
                .GroupBy(error => error.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(error => error.ErrorMessage).ToArray())
        };
    }
}

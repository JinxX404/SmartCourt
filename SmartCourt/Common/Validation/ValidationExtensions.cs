using FluentValidation;
using SmartCourt.Common.Exceptions;

namespace SmartCourt.Common.Validation;

public static class ValidationExtensions
{
    public static async Task ValidateAndThrowBusinessExceptionAsync<T>(
        this IValidator<T> validator,
        T instance,
        CancellationToken cancellationToken = default)
    {
        var result = await validator.ValidateAsync(instance, cancellationToken);
        if (!result.IsValid)
        {
            throw new BusinessException(
                string.Join(
                    " ",
                    result.Errors
                        .Select(error => error.ErrorMessage)
                        .Distinct(StringComparer.Ordinal)));
        }
    }
}

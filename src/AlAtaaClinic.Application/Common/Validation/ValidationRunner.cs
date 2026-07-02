using AlAtaaClinic.Application.Common.Exceptions;

namespace AlAtaaClinic.Application.Common.Validation;

public sealed class ValidationRunner<T>
{
    private readonly IEnumerable<IValidator<T>> _validators;

    public ValidationRunner(IEnumerable<IValidator<T>> validators)
    {
        _validators = validators;
    }

    public void ValidateAndThrow(T instance)
    {
        var errors = _validators
            .SelectMany(validator => validator.Validate(instance).Errors)
            .ToList();

        if (errors.Count > 0)
        {
            throw new ValidationFailedException(errors);
        }
    }
}

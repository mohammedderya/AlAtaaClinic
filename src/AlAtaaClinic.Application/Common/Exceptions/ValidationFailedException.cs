using AlAtaaClinic.Application.Common.Validation;

namespace AlAtaaClinic.Application.Common.Exceptions;

public sealed class ValidationFailedException : AppException
{
    public ValidationFailedException(IReadOnlyList<ValidationError> errors)
        : base("Validation failed.")
    {
        Errors = errors;
    }

    public IReadOnlyList<ValidationError> Errors { get; }
}

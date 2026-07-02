namespace AlAtaaClinic.Application.Common.Validation;

public abstract class ValidatorBase<T> : IValidator<T>
{
    private readonly List<ValidationError> _errors = [];

    public ValidationResult Validate(T instance)
    {
        _errors.Clear();
        ValidateRules(instance);
        return _errors.Count == 0 ? ValidationResult.Success : new ValidationResult([.. _errors]);
    }

    protected abstract void ValidateRules(T instance);

    protected void Required(string? value, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            Add(propertyName, $"{propertyName} is required.");
        }
    }

    protected void MaxLength(string? value, int maxLength, string propertyName)
    {
        if (value?.Length > maxLength)
        {
            Add(propertyName, $"{propertyName} cannot exceed {maxLength} characters.");
        }
    }

    protected void Positive(decimal value, string propertyName)
    {
        if (value <= 0)
        {
            Add(propertyName, $"{propertyName} must be greater than zero.");
        }
    }

    protected void NotNegative(decimal value, string propertyName)
    {
        if (value < 0)
        {
            Add(propertyName, $"{propertyName} cannot be negative.");
        }
    }

    protected void Add(string propertyName, string message)
    {
        _errors.Add(new ValidationError(propertyName, message));
    }
}

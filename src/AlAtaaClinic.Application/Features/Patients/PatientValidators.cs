using AlAtaaClinic.Application.Common.Validation;

namespace AlAtaaClinic.Application.Features.Patients;

public sealed class CreatePatientCommandValidator : ValidatorBase<CreatePatientCommand>
{
    protected override void ValidateRules(CreatePatientCommand instance)
    {
        Required(instance.FileNumber, nameof(instance.FileNumber));
        Required(instance.FullNameArabic, nameof(instance.FullNameArabic));
        MaxLength(instance.FileNumber, 50, nameof(instance.FileNumber));
        MaxLength(instance.NationalId, 50, nameof(instance.NationalId));
        MaxLength(instance.FullNameArabic, 250, nameof(instance.FullNameArabic));
        ValidateDateOfBirth(instance.DateOfBirth);
    }

    private void ValidateDateOfBirth(DateOnly? dateOfBirth)
    {
        if (dateOfBirth > DateOnly.FromDateTime(DateTime.UtcNow))
        {
            Add(nameof(CreatePatientCommand.DateOfBirth), "Date of birth cannot be in the future.");
        }
    }
}

public sealed class UpdatePatientCommandValidator : ValidatorBase<UpdatePatientCommand>
{
    protected override void ValidateRules(UpdatePatientCommand instance)
    {
        Required(instance.FullNameArabic, nameof(instance.FullNameArabic));
        MaxLength(instance.NationalId, 50, nameof(instance.NationalId));
        MaxLength(instance.FullNameArabic, 250, nameof(instance.FullNameArabic));
        ValidateDateOfBirth(instance.DateOfBirth);
    }

    private void ValidateDateOfBirth(DateOnly? dateOfBirth)
    {
        if (dateOfBirth > DateOnly.FromDateTime(DateTime.UtcNow))
        {
            Add(nameof(UpdatePatientCommand.DateOfBirth), "Date of birth cannot be in the future.");
        }
    }
}

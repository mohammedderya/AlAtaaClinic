using AlAtaaClinic.Application.Common.Validation;

namespace AlAtaaClinic.Application.Features.Doctors;

public sealed class CreateDoctorCommandValidator : ValidatorBase<CreateDoctorCommand>
{
    protected override void ValidateRules(CreateDoctorCommand instance)
    {
        Required(instance.Specialty, nameof(instance.Specialty));
        MaxLength(instance.Specialty, 150, nameof(instance.Specialty));
    }
}

public sealed class UpdateDoctorCommandValidator : ValidatorBase<UpdateDoctorCommand>
{
    protected override void ValidateRules(UpdateDoctorCommand instance)
    {
        Required(instance.Specialty, nameof(instance.Specialty));
        MaxLength(instance.Specialty, 150, nameof(instance.Specialty));
    }
}

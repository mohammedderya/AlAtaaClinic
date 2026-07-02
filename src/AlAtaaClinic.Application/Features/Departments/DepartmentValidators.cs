using AlAtaaClinic.Application.Common.Validation;

namespace AlAtaaClinic.Application.Features.Departments;

public sealed class CreateDepartmentCommandValidator : ValidatorBase<CreateDepartmentCommand>
{
    protected override void ValidateRules(CreateDepartmentCommand instance)
    {
        Required(instance.ArabicName, nameof(instance.ArabicName));
        MaxLength(instance.ArabicName, 200, nameof(instance.ArabicName));
    }
}

public sealed class UpdateDepartmentCommandValidator : ValidatorBase<UpdateDepartmentCommand>
{
    protected override void ValidateRules(UpdateDepartmentCommand instance)
    {
        Required(instance.ArabicName, nameof(instance.ArabicName));
        MaxLength(instance.ArabicName, 200, nameof(instance.ArabicName));
    }
}

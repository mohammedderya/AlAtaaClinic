using AlAtaaClinic.Application.Common.Validation;

namespace AlAtaaClinic.Application.Features.Medicines;

public sealed class CreateMedicineCommandValidator : ValidatorBase<CreateMedicineCommand>
{
    protected override void ValidateRules(CreateMedicineCommand instance)
    {
        Required(instance.MedicineCode, nameof(instance.MedicineCode));
        Required(instance.GenericName, nameof(instance.GenericName));
        Required(instance.Unit, nameof(instance.Unit));
        MaxLength(instance.MedicineCode, 50, nameof(instance.MedicineCode));
        MaxLength(instance.GenericName, 250, nameof(instance.GenericName));
    }
}

public sealed class UpdateMedicineCommandValidator : ValidatorBase<UpdateMedicineCommand>
{
    protected override void ValidateRules(UpdateMedicineCommand instance)
    {
        if (instance.Id <= 0) Add(nameof(instance.Id), "Medicine id is required.");
        Required(instance.GenericName, nameof(instance.GenericName));
        Required(instance.Unit, nameof(instance.Unit));
        MaxLength(instance.GenericName, 250, nameof(instance.GenericName));
    }
}

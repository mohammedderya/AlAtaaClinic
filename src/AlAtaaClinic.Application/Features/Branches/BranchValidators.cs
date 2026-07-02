using AlAtaaClinic.Application.Common.Validation;

namespace AlAtaaClinic.Application.Features.Branches;

public sealed class CreateBranchCommandValidator : ValidatorBase<CreateBranchCommand>
{
    protected override void ValidateRules(CreateBranchCommand instance)
    {
        Required(instance.BranchCode, nameof(instance.BranchCode));
        Required(instance.ArabicName, nameof(instance.ArabicName));
        MaxLength(instance.BranchCode, 30, nameof(instance.BranchCode));
        MaxLength(instance.ArabicName, 200, nameof(instance.ArabicName));
    }
}

public sealed class UpdateBranchCommandValidator : ValidatorBase<UpdateBranchCommand>
{
    protected override void ValidateRules(UpdateBranchCommand instance)
    {
        Required(instance.ArabicName, nameof(instance.ArabicName));
        MaxLength(instance.ArabicName, 200, nameof(instance.ArabicName));
    }
}

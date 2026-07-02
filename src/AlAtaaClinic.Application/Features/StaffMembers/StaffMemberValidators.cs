using AlAtaaClinic.Application.Common.Validation;

namespace AlAtaaClinic.Application.Features.StaffMembers;

public sealed class CreateStaffMemberCommandValidator : ValidatorBase<CreateStaffMemberCommand>
{
    protected override void ValidateRules(CreateStaffMemberCommand instance)
    {
        Required(instance.StaffCode, nameof(instance.StaffCode));
        Required(instance.FullNameArabic, nameof(instance.FullNameArabic));
        MaxLength(instance.StaffCode, 50, nameof(instance.StaffCode));
        MaxLength(instance.FullNameArabic, 200, nameof(instance.FullNameArabic));
    }
}

public sealed class UpdateStaffMemberCommandValidator : ValidatorBase<UpdateStaffMemberCommand>
{
    protected override void ValidateRules(UpdateStaffMemberCommand instance)
    {
        Required(instance.FullNameArabic, nameof(instance.FullNameArabic));
        MaxLength(instance.FullNameArabic, 200, nameof(instance.FullNameArabic));
    }
}

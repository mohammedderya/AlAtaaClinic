using AlAtaaClinic.Application.Common.Validation;

namespace AlAtaaClinic.Application.Features.Auth;

public sealed class LoginCommandValidator : ValidatorBase<LoginCommand>
{
    protected override void ValidateRules(LoginCommand instance)
    {
        Required(instance.Username, nameof(instance.Username));
        Required(instance.Password, nameof(instance.Password));
    }
}

public sealed class CreateUserAccountCommandValidator : ValidatorBase<CreateUserAccountCommand>
{
    protected override void ValidateRules(CreateUserAccountCommand instance)
    {
        if (instance.StaffMemberId <= 0) Add(nameof(instance.StaffMemberId), "Staff member is required.");
        Required(instance.Username, nameof(instance.Username));
        Required(instance.Password, nameof(instance.Password));
        MaxLength(instance.Username, 100, nameof(instance.Username));
        ValidatePassword(instance.Password, nameof(instance.Password));
    }

    private void ValidatePassword(string password, string propertyName)
    {
        if (password.Length < 8)
        {
            Add(propertyName, "Password must be at least 8 characters.");
        }
    }
}

public sealed class ChangePasswordCommandValidator : ValidatorBase<ChangePasswordCommand>
{
    protected override void ValidateRules(ChangePasswordCommand instance)
    {
        if (instance.UserAccountId <= 0) Add(nameof(instance.UserAccountId), "User account is required.");
        Required(instance.CurrentPassword, nameof(instance.CurrentPassword));
        Required(instance.NewPassword, nameof(instance.NewPassword));
        ValidatePassword(instance.NewPassword, nameof(instance.NewPassword));
    }

    private void ValidatePassword(string password, string propertyName)
    {
        if (password.Length < 8)
        {
            Add(propertyName, "Password must be at least 8 characters.");
        }
    }
}

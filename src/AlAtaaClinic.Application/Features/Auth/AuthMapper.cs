using AlAtaaClinic.Domain.Security;

namespace AlAtaaClinic.Application.Features.Auth;

public static class AuthMapper
{
    public static UserAccountDto ToDto(this UserAccount user)
    {
        return new UserAccountDto(user.Id, user.StaffMemberId, user.Username, user.IsActive, user.LastLoginAt);
    }
}

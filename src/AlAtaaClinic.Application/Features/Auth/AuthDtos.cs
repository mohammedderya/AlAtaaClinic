namespace AlAtaaClinic.Application.Features.Auth;

public sealed record LoginResultDto(
    long UserAccountId,
    string Username,
    string FullNameArabic,
    IReadOnlyList<string> Permissions,
    bool MustChangePassword);

public sealed record UserAccountDto(
    long Id,
    long StaffMemberId,
    string Username,
    bool IsActive,
    DateTime? LastLoginAt);

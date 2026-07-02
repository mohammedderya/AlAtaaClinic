namespace AlAtaaClinic.Application.Features.StaffMembers;

public sealed record StaffMemberDto(
    long Id,
    long BranchId,
    string? BranchName,
    long? DepartmentId,
    string? DepartmentName,
    string StaffCode,
    string FullNameArabic,
    string? FullNameEnglish,
    string? PhoneNumber,
    string? JobTitle,
    bool IsActive);

public sealed record StaffMemberListDto(
    long Id,
    string StaffCode,
    string FullNameArabic,
    string? FullNameEnglish,
    string? JobTitle,
    string? PhoneNumber,
    bool IsActive);

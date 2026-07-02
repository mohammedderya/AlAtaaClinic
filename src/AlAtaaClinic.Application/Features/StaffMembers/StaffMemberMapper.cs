using AlAtaaClinic.Domain.Core;

namespace AlAtaaClinic.Application.Features.StaffMembers;

public static class StaffMemberMapper
{
    public static StaffMemberDto ToDto(this StaffMember staffMember)
    {
        return new StaffMemberDto(
            staffMember.Id,
            staffMember.BranchId,
            staffMember.Branch?.ArabicName,
            staffMember.DepartmentId,
            staffMember.Department?.ArabicName,
            staffMember.StaffCode,
            staffMember.FullNameArabic,
            staffMember.FullNameEnglish,
            staffMember.PhoneNumber,
            staffMember.JobTitle,
            staffMember.IsActive);
    }

    public static StaffMemberListDto ToListDto(this StaffMember staffMember)
    {
        return new StaffMemberListDto(
            staffMember.Id,
            staffMember.StaffCode,
            staffMember.FullNameArabic,
            staffMember.FullNameEnglish,
            staffMember.JobTitle,
            staffMember.PhoneNumber,
            staffMember.IsActive);
    }
}

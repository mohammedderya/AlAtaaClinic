using AlAtaaClinic.Application.Common.Messaging;
using AlAtaaClinic.Application.Common.Models;

namespace AlAtaaClinic.Application.Features.StaffMembers;

public sealed record CreateStaffMemberCommand(
    long BranchId,
    long? DepartmentId,
    string StaffCode,
    string FullNameArabic,
    string? FullNameEnglish,
    string? PhoneNumber,
    string? JobTitle) : ICommand<StaffMemberDto>;

public sealed record UpdateStaffMemberCommand(
    long Id,
    long BranchId,
    long? DepartmentId,
    string FullNameArabic,
    string? FullNameEnglish,
    string? PhoneNumber,
    string? JobTitle,
    bool IsActive) : ICommand<StaffMemberDto>;

public sealed record DeleteStaffMemberCommand(long Id) : ICommand<OperationResult>;

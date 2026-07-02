namespace AlAtaaClinic.Application.Features.Departments;

public sealed record DepartmentDto(
    long Id,
    long BranchId,
    string BranchName,
    string ArabicName,
    string? EnglishName,
    bool IsActive);

public sealed record DepartmentListDto(
    long Id,
    string ArabicName,
    string BranchName,
    bool IsActive);

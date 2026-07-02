namespace AlAtaaClinic.Application.Features.Branches;

public sealed record BranchDto(
    long Id,
    string BranchCode,
    string ArabicName,
    string? EnglishName,
    string? PhoneNumber,
    string? AddressLine,
    bool IsActive);

public sealed record BranchListDto(
    long Id,
    string BranchCode,
    string ArabicName,
    string? PhoneNumber,
    bool IsActive);

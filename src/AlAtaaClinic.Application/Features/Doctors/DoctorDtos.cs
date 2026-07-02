namespace AlAtaaClinic.Application.Features.Doctors;

public sealed record DoctorDto(
    long Id,
    long StaffMemberId,
    string? StaffMemberName,
    string? StaffCode,
    string Specialty,
    string? LicenseNumber,
    bool IsActive);

public sealed record DoctorListDto(
    long Id,
    string? StaffMemberName,
    string? StaffCode,
    string Specialty,
    string? LicenseNumber,
    bool IsActive);

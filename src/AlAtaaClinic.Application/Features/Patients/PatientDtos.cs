using AlAtaaClinic.Domain.Enums;

namespace AlAtaaClinic.Application.Features.Patients;

public sealed record PatientDto(
    long Id,
    string FileNumber,
    string? NationalId,
    string FullNameArabic,
    string? FullNameEnglish,
    Gender Gender,
    DateOnly? DateOfBirth,
    string? PhoneNumber,
    string? AlternativePhoneNumber,
    string? AddressLine,
    string? BloodType,
    bool IsActive);

public sealed record PatientListDto(
    long Id,
    string FileNumber,
    string FullNameArabic,
    string? PhoneNumber,
    Gender Gender,
    bool IsActive);

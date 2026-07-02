using AlAtaaClinic.Application.Common.Messaging;
using AlAtaaClinic.Application.Common.Models;
using AlAtaaClinic.Domain.Enums;

namespace AlAtaaClinic.Application.Features.Patients;

public sealed record CreatePatientCommand(
    string FileNumber,
    string? NationalId,
    string FullNameArabic,
    string? FullNameEnglish,
    Gender Gender,
    DateOnly? DateOfBirth,
    string? PhoneNumber,
    string? AlternativePhoneNumber,
    string? AddressLine,
    string? BloodType) : ICommand<PatientDto>;

public sealed record UpdatePatientCommand(
    long Id,
    string? NationalId,
    string FullNameArabic,
    string? FullNameEnglish,
    Gender Gender,
    DateOnly? DateOfBirth,
    string? PhoneNumber,
    string? AlternativePhoneNumber,
    string? AddressLine,
    string? BloodType,
    bool IsActive) : ICommand<PatientDto>;

public sealed record DeletePatientCommand(long Id) : ICommand<OperationResult>;

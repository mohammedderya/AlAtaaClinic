using AlAtaaClinic.Application.Common.Messaging;
using AlAtaaClinic.Application.Common.Models;

namespace AlAtaaClinic.Application.Features.Doctors;

public sealed record CreateDoctorCommand(
    long StaffMemberId,
    string Specialty,
    string? LicenseNumber) : ICommand<DoctorDto>;

public sealed record UpdateDoctorCommand(
    long Id,
    string Specialty,
    string? LicenseNumber,
    bool IsActive) : ICommand<DoctorDto>;

public sealed record DeleteDoctorCommand(long Id) : ICommand<OperationResult>;

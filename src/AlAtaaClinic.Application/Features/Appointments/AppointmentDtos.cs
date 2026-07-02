using AlAtaaClinic.Domain.Enums;

namespace AlAtaaClinic.Application.Features.Appointments;

public sealed record AppointmentDto(
    long Id,
    long BranchId,
    long PatientId,
    long? DoctorId,
    DateTime AppointmentStart,
    DateTime AppointmentEnd,
    AppointmentStatus Status,
    string? Notes);

public sealed record AppointmentListDto(
    long Id,
    long PatientId,
    long? DoctorId,
    DateTime AppointmentStart,
    DateTime AppointmentEnd,
    AppointmentStatus Status);

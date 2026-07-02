using AlAtaaClinic.Application.Common.Messaging;
using AlAtaaClinic.Application.Common.Models;
using AlAtaaClinic.Domain.Enums;

namespace AlAtaaClinic.Application.Features.Appointments;

public sealed record CreateAppointmentCommand(
    long BranchId,
    long PatientId,
    long? DoctorId,
    DateTime AppointmentStart,
    DateTime AppointmentEnd,
    string? Notes) : ICommand<AppointmentDto>;

public sealed record UpdateAppointmentCommand(
    long Id,
    long BranchId,
    long PatientId,
    long? DoctorId,
    DateTime AppointmentStart,
    DateTime AppointmentEnd,
    AppointmentStatus Status,
    string? Notes) : ICommand<AppointmentDto>;

public sealed record DeleteAppointmentCommand(long Id) : ICommand<OperationResult>;

using AlAtaaClinic.Application.Common.Messaging;
using AlAtaaClinic.Application.Common.Models;

namespace AlAtaaClinic.Application.Features.Appointments;

public sealed record GetAppointmentByIdQuery(long Id) : IQuery<AppointmentDto>;

public sealed record SearchAppointmentsQuery(
    long? DoctorId,
    DateTime? From,
    DateTime? To,
    PageRequest Page) : IQuery<PagedResult<AppointmentListDto>>;

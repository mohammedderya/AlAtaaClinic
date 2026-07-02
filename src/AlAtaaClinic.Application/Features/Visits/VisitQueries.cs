using AlAtaaClinic.Application.Common.Messaging;
using AlAtaaClinic.Application.Common.Models;

namespace AlAtaaClinic.Application.Features.Visits;

public sealed record GetVisitByIdQuery(long Id) : IQuery<VisitDto>;

public sealed record SearchVisitsQuery(
    long? PatientId,
    long? DoctorId,
    DateTime? From,
    DateTime? To,
    PageRequest Page) : IQuery<PagedResult<VisitListDto>>;

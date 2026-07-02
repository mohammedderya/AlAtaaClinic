using AlAtaaClinic.Application.Common.Messaging;
using AlAtaaClinic.Application.Common.Models;

namespace AlAtaaClinic.Application.Features.Patients;

public sealed record GetPatientByIdQuery(long Id) : IQuery<PatientDto>;

public sealed record SearchPatientsQuery(string? SearchText, PageRequest Page) : IQuery<PagedResult<PatientListDto>>;

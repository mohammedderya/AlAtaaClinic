using AlAtaaClinic.Application.Common.Messaging;
using AlAtaaClinic.Application.Common.Models;

namespace AlAtaaClinic.Application.Features.Doctors;

public sealed record GetDoctorByIdQuery(long Id) : IQuery<DoctorDto>;

public sealed record SearchDoctorsQuery(string? SearchText, PageRequest Page) : IQuery<PagedResult<DoctorListDto>>;

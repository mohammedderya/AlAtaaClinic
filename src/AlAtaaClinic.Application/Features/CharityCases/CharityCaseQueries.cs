using AlAtaaClinic.Application.Common.Messaging;
using AlAtaaClinic.Application.Common.Models;

namespace AlAtaaClinic.Application.Features.CharityCases;

public sealed record GetCharityCaseByIdQuery(long Id) : IQuery<CharityCaseDto>;

public sealed record SearchCharityCasesQuery(long? PatientId, string? CaseNumber, PageRequest Page) : IQuery<PagedResult<CharityCaseListDto>>;

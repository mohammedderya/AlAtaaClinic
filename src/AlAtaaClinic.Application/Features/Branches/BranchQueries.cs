using AlAtaaClinic.Application.Common.Messaging;
using AlAtaaClinic.Application.Common.Models;

namespace AlAtaaClinic.Application.Features.Branches;

public sealed record GetBranchByIdQuery(long Id) : IQuery<BranchDto>;

public sealed record SearchBranchesQuery(string? SearchText, PageRequest Page) : IQuery<PagedResult<BranchListDto>>;

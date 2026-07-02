using AlAtaaClinic.Application.Common.Messaging;
using AlAtaaClinic.Application.Common.Models;

namespace AlAtaaClinic.Application.Features.StaffMembers;

public sealed record GetStaffMemberByIdQuery(long Id) : IQuery<StaffMemberDto>;

public sealed record SearchStaffMembersQuery(string? SearchText, PageRequest Page) : IQuery<PagedResult<StaffMemberListDto>>;

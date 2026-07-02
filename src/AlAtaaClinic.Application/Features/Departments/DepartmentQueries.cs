using AlAtaaClinic.Application.Common.Messaging;
using AlAtaaClinic.Application.Common.Models;

namespace AlAtaaClinic.Application.Features.Departments;

public sealed record GetDepartmentByIdQuery(long Id) : IQuery<DepartmentDto>;

public sealed record SearchDepartmentsQuery(string? SearchText, long? BranchId, PageRequest Page) : IQuery<PagedResult<DepartmentListDto>>;

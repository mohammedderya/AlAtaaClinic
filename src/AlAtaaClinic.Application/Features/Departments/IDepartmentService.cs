using AlAtaaClinic.Application.Common.Models;

namespace AlAtaaClinic.Application.Features.Departments;

public interface IDepartmentService
{
    Task<DepartmentDto> CreateAsync(CreateDepartmentCommand command, CancellationToken cancellationToken = default);
    Task<DepartmentDto> UpdateAsync(UpdateDepartmentCommand command, CancellationToken cancellationToken = default);
    Task<OperationResult> DeleteAsync(DeleteDepartmentCommand command, CancellationToken cancellationToken = default);
    Task<DepartmentDto> GetByIdAsync(GetDepartmentByIdQuery query, CancellationToken cancellationToken = default);
    Task<PagedResult<DepartmentListDto>> SearchAsync(SearchDepartmentsQuery query, CancellationToken cancellationToken = default);
}

using AlAtaaClinic.Application.Common.Models;

namespace AlAtaaClinic.Application.Features.StaffMembers;

public interface IStaffMemberService
{
    Task<StaffMemberDto> CreateAsync(CreateStaffMemberCommand command, CancellationToken cancellationToken = default);
    Task<StaffMemberDto> UpdateAsync(UpdateStaffMemberCommand command, CancellationToken cancellationToken = default);
    Task<OperationResult> DeleteAsync(DeleteStaffMemberCommand command, CancellationToken cancellationToken = default);
    Task<StaffMemberDto> GetByIdAsync(GetStaffMemberByIdQuery query, CancellationToken cancellationToken = default);
    Task<PagedResult<StaffMemberListDto>> SearchAsync(SearchStaffMembersQuery query, CancellationToken cancellationToken = default);
}

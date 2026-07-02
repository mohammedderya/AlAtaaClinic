using AlAtaaClinic.Application.Common.Models;

namespace AlAtaaClinic.Application.Features.Branches;

public interface IBranchService
{
    Task<BranchDto> CreateAsync(CreateBranchCommand command, CancellationToken cancellationToken = default);
    Task<BranchDto> UpdateAsync(UpdateBranchCommand command, CancellationToken cancellationToken = default);
    Task<OperationResult> DeleteAsync(DeleteBranchCommand command, CancellationToken cancellationToken = default);
    Task<BranchDto> GetByIdAsync(GetBranchByIdQuery query, CancellationToken cancellationToken = default);
    Task<PagedResult<BranchListDto>> SearchAsync(SearchBranchesQuery query, CancellationToken cancellationToken = default);
}

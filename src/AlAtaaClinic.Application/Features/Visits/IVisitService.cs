using AlAtaaClinic.Application.Common.Models;

namespace AlAtaaClinic.Application.Features.Visits;

public interface IVisitService
{
    Task<VisitDto> CreateAsync(CreateVisitCommand command, CancellationToken cancellationToken = default);
    Task<VisitDto> UpdateAsync(UpdateVisitCommand command, CancellationToken cancellationToken = default);
    Task<OperationResult> DeleteAsync(DeleteVisitCommand command, CancellationToken cancellationToken = default);
    Task<VisitDto> GetByIdAsync(GetVisitByIdQuery query, CancellationToken cancellationToken = default);
    Task<PagedResult<VisitListDto>> SearchAsync(SearchVisitsQuery query, CancellationToken cancellationToken = default);
}

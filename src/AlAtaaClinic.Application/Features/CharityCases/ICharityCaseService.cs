using AlAtaaClinic.Application.Common.Models;

namespace AlAtaaClinic.Application.Features.CharityCases;

public interface ICharityCaseService
{
    Task<CharityCaseDto> CreateAsync(CreateCharityCaseCommand command, CancellationToken cancellationToken = default);
    Task<CharityCaseDto> UpdateAsync(UpdateCharityCaseCommand command, CancellationToken cancellationToken = default);
    Task<OperationResult> DeleteAsync(DeleteCharityCaseCommand command, CancellationToken cancellationToken = default);
    Task<CharityCaseDto> GetByIdAsync(GetCharityCaseByIdQuery query, CancellationToken cancellationToken = default);
    Task<PagedResult<CharityCaseListDto>> SearchAsync(SearchCharityCasesQuery query, CancellationToken cancellationToken = default);
}

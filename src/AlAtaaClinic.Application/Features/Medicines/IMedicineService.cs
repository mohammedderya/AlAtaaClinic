using AlAtaaClinic.Application.Common.Models;

namespace AlAtaaClinic.Application.Features.Medicines;

public interface IMedicineService
{
    Task<MedicineDto> CreateAsync(CreateMedicineCommand command, CancellationToken cancellationToken = default);
    Task<MedicineDto> UpdateAsync(UpdateMedicineCommand command, CancellationToken cancellationToken = default);
    Task<OperationResult> DeleteAsync(DeleteMedicineCommand command, CancellationToken cancellationToken = default);
    Task<MedicineDto> GetByIdAsync(GetMedicineByIdQuery query, CancellationToken cancellationToken = default);
    Task<PagedResult<MedicineListDto>> SearchAsync(SearchMedicinesQuery query, CancellationToken cancellationToken = default);
}

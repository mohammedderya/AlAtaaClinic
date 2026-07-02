using AlAtaaClinic.Application.Common.Models;

namespace AlAtaaClinic.Application.Features.Patients;

public interface IPatientService
{
    Task<PatientDto> CreateAsync(CreatePatientCommand command, CancellationToken cancellationToken = default);
    Task<PatientDto> UpdateAsync(UpdatePatientCommand command, CancellationToken cancellationToken = default);
    Task<OperationResult> DeleteAsync(DeletePatientCommand command, CancellationToken cancellationToken = default);
    Task<PatientDto> GetByIdAsync(GetPatientByIdQuery query, CancellationToken cancellationToken = default);
    Task<PagedResult<PatientListDto>> SearchAsync(SearchPatientsQuery query, CancellationToken cancellationToken = default);
}

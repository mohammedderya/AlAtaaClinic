using AlAtaaClinic.Application.Common.Models;

namespace AlAtaaClinic.Application.Features.Doctors;

public interface IDoctorService
{
    Task<DoctorDto> CreateAsync(CreateDoctorCommand command, CancellationToken cancellationToken = default);
    Task<DoctorDto> UpdateAsync(UpdateDoctorCommand command, CancellationToken cancellationToken = default);
    Task<OperationResult> DeleteAsync(DeleteDoctorCommand command, CancellationToken cancellationToken = default);
    Task<DoctorDto> GetByIdAsync(GetDoctorByIdQuery query, CancellationToken cancellationToken = default);
    Task<PagedResult<DoctorListDto>> SearchAsync(SearchDoctorsQuery query, CancellationToken cancellationToken = default);
}

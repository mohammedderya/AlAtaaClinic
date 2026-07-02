using AlAtaaClinic.Application.Common.Models;

namespace AlAtaaClinic.Application.Features.Appointments;

public interface IAppointmentService
{
    Task<AppointmentDto> CreateAsync(CreateAppointmentCommand command, CancellationToken cancellationToken = default);
    Task<AppointmentDto> UpdateAsync(UpdateAppointmentCommand command, CancellationToken cancellationToken = default);
    Task<OperationResult> DeleteAsync(DeleteAppointmentCommand command, CancellationToken cancellationToken = default);
    Task<AppointmentDto> GetByIdAsync(GetAppointmentByIdQuery query, CancellationToken cancellationToken = default);
    Task<PagedResult<AppointmentListDto>> SearchAsync(SearchAppointmentsQuery query, CancellationToken cancellationToken = default);
}

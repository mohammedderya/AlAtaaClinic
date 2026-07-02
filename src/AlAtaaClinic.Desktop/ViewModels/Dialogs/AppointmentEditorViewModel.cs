using AlAtaaClinic.Application.Common.ExceptionHandling;
using AlAtaaClinic.Application.Features.Appointments;
using AlAtaaClinic.Desktop.Common;
using AlAtaaClinic.Domain.Enums;

namespace AlAtaaClinic.Desktop.ViewModels.Dialogs;

public sealed class AppointmentEditorViewModel : DialogViewModelBase
{
    private readonly IAppointmentService _appointmentService;

    public AppointmentEditorViewModel(IAppointmentService appointmentService, IExceptionHandler exceptionHandler, AppointmentDto? appointment = null)
        : base(exceptionHandler)
    {
        _appointmentService = appointmentService;
        AppointmentStart = DateTime.Today.AddHours(9);
        AppointmentEnd = DateTime.Today.AddHours(9.5);
        Load(appointment);
    }

    public long Id { get; private set; }
    public bool IsEditMode => Id > 0;
    public AppointmentStatus[] Statuses { get; } = Enum.GetValues<AppointmentStatus>();
    public long BranchId { get; set; } = 1;
    public long PatientId { get; set; }
    public long? DoctorId { get; set; }
    public DateTime AppointmentStart { get; set; }
    public DateTime AppointmentEnd { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;
    public string? Notes { get; set; }

    protected override Task SaveAsync()
    {
        return RunAsync(async () =>
        {
            if (IsEditMode)
            {
                await _appointmentService.UpdateAsync(ToUpdateCommand());
                Close(true);
                return;
            }

            await _appointmentService.CreateAsync(ToCreateCommand());
            Close(true);
        });
    }

    private CreateAppointmentCommand ToCreateCommand()
    {
        return new CreateAppointmentCommand(BranchId, PatientId, DoctorId, AppointmentStart, AppointmentEnd, Notes);
    }

    private UpdateAppointmentCommand ToUpdateCommand()
    {
        return new UpdateAppointmentCommand(Id, BranchId, PatientId, DoctorId, AppointmentStart, AppointmentEnd, Status, Notes);
    }

    private void Load(AppointmentDto? appointment)
    {
        if (appointment is null)
        {
            return;
        }

        Id = appointment.Id;
        BranchId = appointment.BranchId;
        PatientId = appointment.PatientId;
        DoctorId = appointment.DoctorId;
        AppointmentStart = appointment.AppointmentStart;
        AppointmentEnd = appointment.AppointmentEnd;
        Status = appointment.Status;
        Notes = appointment.Notes;
    }
}

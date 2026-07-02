using AlAtaaClinic.Domain.Clinical;

namespace AlAtaaClinic.Application.Features.Appointments;

public static class AppointmentMapper
{
    public static AppointmentDto ToDto(this Appointment appointment)
    {
        return new AppointmentDto(
            appointment.Id,
            appointment.BranchId,
            appointment.PatientId,
            appointment.DoctorId,
            appointment.AppointmentStart,
            appointment.AppointmentEnd,
            appointment.Status,
            appointment.Notes);
    }

    public static AppointmentListDto ToListDto(this Appointment appointment)
    {
        return new AppointmentListDto(
            appointment.Id,
            appointment.PatientId,
            appointment.DoctorId,
            appointment.AppointmentStart,
            appointment.AppointmentEnd,
            appointment.Status);
    }
}

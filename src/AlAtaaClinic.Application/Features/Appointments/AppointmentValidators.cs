using AlAtaaClinic.Application.Common.Validation;

namespace AlAtaaClinic.Application.Features.Appointments;

public sealed class CreateAppointmentCommandValidator : ValidatorBase<CreateAppointmentCommand>
{
    protected override void ValidateRules(CreateAppointmentCommand instance)
    {
        ValidateIds(instance.BranchId, instance.PatientId);
        ValidateTime(instance.AppointmentStart, instance.AppointmentEnd);
        MaxLength(instance.Notes, 500, nameof(instance.Notes));
    }

    private void ValidateIds(long branchId, long patientId)
    {
        if (branchId <= 0) Add(nameof(CreateAppointmentCommand.BranchId), "Branch is required.");
        if (patientId <= 0) Add(nameof(CreateAppointmentCommand.PatientId), "Patient is required.");
    }

    private void ValidateTime(DateTime start, DateTime end)
    {
        if (end <= start)
        {
            Add(nameof(CreateAppointmentCommand.AppointmentEnd), "Appointment end must be after start.");
        }
    }
}

public sealed class UpdateAppointmentCommandValidator : ValidatorBase<UpdateAppointmentCommand>
{
    protected override void ValidateRules(UpdateAppointmentCommand instance)
    {
        if (instance.Id <= 0) Add(nameof(instance.Id), "Appointment id is required.");
        if (instance.BranchId <= 0) Add(nameof(instance.BranchId), "Branch is required.");
        if (instance.PatientId <= 0) Add(nameof(instance.PatientId), "Patient is required.");
        if (instance.AppointmentEnd <= instance.AppointmentStart)
        {
            Add(nameof(instance.AppointmentEnd), "Appointment end must be after start.");
        }

        MaxLength(instance.Notes, 500, nameof(instance.Notes));
    }
}

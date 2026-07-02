using AlAtaaClinic.Application.Common.Validation;

namespace AlAtaaClinic.Application.Features.Visits;

public sealed class CreateVisitCommandValidator : ValidatorBase<CreateVisitCommand>
{
    protected override void ValidateRules(CreateVisitCommand instance)
    {
        if (instance.BranchId <= 0) Add(nameof(instance.BranchId), "Branch is required.");
        if (instance.PatientId <= 0) Add(nameof(instance.PatientId), "Patient is required.");
        if (instance.DoctorId <= 0) Add(nameof(instance.DoctorId), "Doctor is required.");
        MaxLength(instance.ChiefComplaint, 1000, nameof(instance.ChiefComplaint));
        ValidateCollections(instance);
    }

    private void ValidateCollections(CreateVisitCommand instance)
    {
        ValidateDiagnoses(instance.Diagnoses);
        ValidatePrescriptions(instance.Prescriptions);
        ValidateMedicalOrders(instance.MedicalOrders);
    }

    private void ValidateDiagnoses(IEnumerable<DiagnosisDto> diagnoses)
    {
        foreach (var diagnosis in diagnoses)
        {
            Required(diagnosis.Description, nameof(DiagnosisDto.Description));
        }
    }

    private void ValidatePrescriptions(IEnumerable<PrescriptionDto> prescriptions)
    {
        foreach (var prescription in prescriptions)
        {
            if (prescription.DoctorId <= 0) Add(nameof(PrescriptionDto.DoctorId), "Prescription doctor is required.");
            foreach (var item in prescription.Items) ValidatePrescriptionItem(item);
        }
    }

    private void ValidatePrescriptionItem(PrescriptionItemDto item)
    {
        Required(item.MedicineName, nameof(PrescriptionItemDto.MedicineName));
        Required(item.Dosage, nameof(PrescriptionItemDto.Dosage));
        Required(item.Frequency, nameof(PrescriptionItemDto.Frequency));
        Required(item.Duration, nameof(PrescriptionItemDto.Duration));
    }

    private void ValidateMedicalOrders(IEnumerable<MedicalOrderDto> orders)
    {
        foreach (var order in orders)
        {
            Required(order.OrderName, nameof(MedicalOrderDto.OrderName));
        }
    }
}

public sealed class UpdateVisitCommandValidator : ValidatorBase<UpdateVisitCommand>
{
    protected override void ValidateRules(UpdateVisitCommand instance)
    {
        if (instance.Id <= 0) Add(nameof(instance.Id), "Visit id is required.");
        MaxLength(instance.ChiefComplaint, 1000, nameof(instance.ChiefComplaint));
        ValidateClosedAt(instance);
    }

    private void ValidateClosedAt(UpdateVisitCommand instance)
    {
        if (instance.ClosedAt > DateTime.UtcNow.AddMinutes(5))
        {
            Add(nameof(instance.ClosedAt), "Closed time cannot be in the future.");
        }
    }
}

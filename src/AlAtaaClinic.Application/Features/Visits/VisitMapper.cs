using AlAtaaClinic.Domain.Clinical;

namespace AlAtaaClinic.Application.Features.Visits;

public static class VisitMapper
{
    public static VisitDto ToDto(this Visit visit)
    {
        return new VisitDto(
            visit.Id,
            visit.BranchId,
            visit.PatientId,
            visit.DoctorId,
            visit.AppointmentId,
            visit.VisitDate,
            visit.Status,
            visit.ChiefComplaint,
            visit.ClinicalNotes,
            visit.ClosedAt,
            visit.VitalSigns.Select(ToDto).ToList(),
            visit.Diagnoses.Select(ToDto).ToList(),
            visit.Procedures.Select(ToDto).ToList(),
            visit.Prescriptions.Select(ToDto).ToList(),
            visit.MedicalOrders.Select(ToDto).ToList());
    }

    public static VisitListDto ToListDto(this Visit visit)
    {
        return new VisitListDto(
            visit.Id,
            visit.PatientId,
            visit.DoctorId,
            visit.VisitDate,
            visit.Status,
            visit.ChiefComplaint);
    }

    public static VitalSign ToEntity(this VitalSignDto dto)
    {
        return new VitalSign
        {
            SystolicPressure = dto.SystolicPressure,
            DiastolicPressure = dto.DiastolicPressure,
            Temperature = dto.Temperature,
            WeightKg = dto.WeightKg,
            HeightCm = dto.HeightCm,
            Pulse = dto.Pulse
        };
    }

    public static Diagnosis ToEntity(this DiagnosisDto dto)
    {
        return new Diagnosis
        {
            DiagnosisCode = dto.DiagnosisCode,
            Description = dto.Description,
            IsPrimary = dto.IsPrimary
        };
    }

    public static ClinicalProcedure ToEntity(this ClinicalProcedureDto dto)
    {
        return new ClinicalProcedure
        {
            ProcedureName = dto.ProcedureName,
            Notes = dto.Notes
        };
    }

    public static Prescription ToEntity(this PrescriptionDto dto)
    {
        return new Prescription
        {
            DoctorId = dto.DoctorId,
            Notes = dto.Notes,
            Items = dto.Items.Select(ToEntity).ToList()
        };
    }

    public static MedicalOrder ToEntity(this MedicalOrderDto dto)
    {
        return new MedicalOrder
        {
            OrderType = dto.OrderType,
            OrderName = dto.OrderName,
            Status = dto.Status
        };
    }

    private static VitalSignDto ToDto(VitalSign vitalSign)
    {
        return new VitalSignDto(
            vitalSign.SystolicPressure,
            vitalSign.DiastolicPressure,
            vitalSign.Temperature,
            vitalSign.WeightKg,
            vitalSign.HeightCm,
            vitalSign.Pulse);
    }

    private static DiagnosisDto ToDto(Diagnosis diagnosis)
    {
        return new DiagnosisDto(diagnosis.DiagnosisCode, diagnosis.Description, diagnosis.IsPrimary);
    }

    private static ClinicalProcedureDto ToDto(ClinicalProcedure procedure)
    {
        return new ClinicalProcedureDto(procedure.ProcedureName, procedure.Notes);
    }

    private static PrescriptionDto ToDto(Prescription prescription)
    {
        return new PrescriptionDto(prescription.DoctorId, prescription.Notes, prescription.Items.Select(ToDto).ToList());
    }

    private static PrescriptionItem ToEntity(PrescriptionItemDto dto)
    {
        return new PrescriptionItem
        {
            MedicineName = dto.MedicineName,
            Dosage = dto.Dosage,
            Frequency = dto.Frequency,
            Duration = dto.Duration,
            Instructions = dto.Instructions,
            Quantity = dto.Quantity
        };
    }

    private static PrescriptionItemDto ToDto(PrescriptionItem item)
    {
        return new PrescriptionItemDto(item.MedicineName, item.Dosage, item.Frequency, item.Duration, item.Instructions, item.Quantity);
    }

    private static MedicalOrderDto ToDto(MedicalOrder order)
    {
        return new MedicalOrderDto(order.OrderType, order.OrderName, order.Status);
    }
}

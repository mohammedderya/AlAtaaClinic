using AlAtaaClinic.Domain.Enums;

namespace AlAtaaClinic.Application.Features.Visits;

public sealed record VitalSignDto(
    int? SystolicPressure,
    int? DiastolicPressure,
    decimal? Temperature,
    decimal? WeightKg,
    decimal? HeightCm,
    int? Pulse);

public sealed record DiagnosisDto(string? DiagnosisCode, string Description, bool IsPrimary);

public sealed record ClinicalProcedureDto(string ProcedureName, string? Notes);

public sealed record PrescriptionItemDto(
    string MedicineName,
    string Dosage,
    string Frequency,
    string Duration,
    string? Instructions,
    decimal? Quantity);

public sealed record PrescriptionDto(long DoctorId, string? Notes, IReadOnlyList<PrescriptionItemDto> Items);

public sealed record MedicalOrderDto(MedicalOrderType OrderType, string OrderName, MedicalOrderStatus Status);

public sealed record VisitDto(
    long Id,
    long BranchId,
    long PatientId,
    long DoctorId,
    long? AppointmentId,
    DateTime VisitDate,
    VisitStatus Status,
    string? ChiefComplaint,
    string? ClinicalNotes,
    DateTime? ClosedAt,
    IReadOnlyList<VitalSignDto> VitalSigns,
    IReadOnlyList<DiagnosisDto> Diagnoses,
    IReadOnlyList<ClinicalProcedureDto> Procedures,
    IReadOnlyList<PrescriptionDto> Prescriptions,
    IReadOnlyList<MedicalOrderDto> MedicalOrders);

public sealed record VisitListDto(
    long Id,
    long PatientId,
    long DoctorId,
    DateTime VisitDate,
    VisitStatus Status,
    string? ChiefComplaint);

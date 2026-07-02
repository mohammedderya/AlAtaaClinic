using AlAtaaClinic.Application.Common.Messaging;
using AlAtaaClinic.Application.Common.Models;
using AlAtaaClinic.Domain.Enums;

namespace AlAtaaClinic.Application.Features.Visits;

public sealed record CreateVisitCommand(
    long BranchId,
    long PatientId,
    long DoctorId,
    long? AppointmentId,
    string? ChiefComplaint,
    string? ClinicalNotes,
    IReadOnlyList<VitalSignDto> VitalSigns,
    IReadOnlyList<DiagnosisDto> Diagnoses,
    IReadOnlyList<ClinicalProcedureDto> Procedures,
    IReadOnlyList<PrescriptionDto> Prescriptions,
    IReadOnlyList<MedicalOrderDto> MedicalOrders) : ICommand<VisitDto>;

public sealed record UpdateVisitCommand(
    long Id,
    VisitStatus Status,
    string? ChiefComplaint,
    string? ClinicalNotes,
    DateTime? ClosedAt,
    IReadOnlyList<VitalSignDto> VitalSigns,
    IReadOnlyList<DiagnosisDto> Diagnoses,
    IReadOnlyList<ClinicalProcedureDto> Procedures,
    IReadOnlyList<PrescriptionDto> Prescriptions,
    IReadOnlyList<MedicalOrderDto> MedicalOrders) : ICommand<VisitDto>;

public sealed record DeleteVisitCommand(long Id) : ICommand<OperationResult>;

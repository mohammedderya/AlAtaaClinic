using AlAtaaClinic.Domain.Common;
using AlAtaaClinic.Domain.Core;
using AlAtaaClinic.Domain.Enums;

namespace AlAtaaClinic.Domain.Clinical;

public sealed class Appointment : AggregateRoot
{
    public long BranchId { get; set; }
    public long PatientId { get; set; }
    public long? DoctorId { get; set; }
    public DateTime AppointmentStart { get; set; }
    public DateTime AppointmentEnd { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;
    public string? Notes { get; set; }

    public Branch? Branch { get; set; }
    public Patient? Patient { get; set; }
    public Doctor? Doctor { get; set; }
}

public sealed class Visit : AggregateRoot
{
    public long BranchId { get; set; }
    public long PatientId { get; set; }
    public long DoctorId { get; set; }
    public long? AppointmentId { get; set; }
    public DateTime VisitDate { get; set; } = DateTime.UtcNow;
    public VisitStatus Status { get; set; } = VisitStatus.Open;
    public string? ChiefComplaint { get; set; }
    public string? ClinicalNotes { get; set; }
    public DateTime? ClosedAt { get; set; }

    public Branch? Branch { get; set; }
    public Patient? Patient { get; set; }
    public Doctor? Doctor { get; set; }
    public Appointment? Appointment { get; set; }
    public List<VitalSign> VitalSigns { get; set; } = [];
    public List<Diagnosis> Diagnoses { get; set; } = [];
    public List<ClinicalProcedure> Procedures { get; set; } = [];
    public List<Prescription> Prescriptions { get; set; } = [];
    public List<MedicalOrder> MedicalOrders { get; set; } = [];
}

public sealed class VitalSign : Entity<long>
{
    public long VisitId { get; set; }
    public long? RecordedByStaffMemberId { get; set; }
    public int? SystolicPressure { get; set; }
    public int? DiastolicPressure { get; set; }
    public decimal? Temperature { get; set; }
    public decimal? WeightKg { get; set; }
    public decimal? HeightCm { get; set; }
    public int? Pulse { get; set; }
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

    public Visit? Visit { get; set; }
    public StaffMember? RecordedByStaffMember { get; set; }
}

public sealed class Diagnosis : Entity<long>
{
    public long VisitId { get; set; }
    public string? DiagnosisCode { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }

    public Visit? Visit { get; set; }
}

public sealed class ClinicalProcedure : Entity<long>
{
    public long VisitId { get; set; }
    public string ProcedureName { get; set; } = string.Empty;
    public string? Notes { get; set; }

    public Visit? Visit { get; set; }
}

public sealed class Prescription : Entity<long>
{
    public long VisitId { get; set; }
    public long DoctorId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }

    public Visit? Visit { get; set; }
    public Doctor? Doctor { get; set; }
    public List<PrescriptionItem> Items { get; set; } = [];
}

public sealed class PrescriptionItem : Entity<long>
{
    public long PrescriptionId { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public string? Instructions { get; set; }
    public decimal? Quantity { get; set; }

    public Prescription? Prescription { get; set; }
}

public sealed class MedicalOrder : Entity<long>
{
    public long VisitId { get; set; }
    public MedicalOrderType OrderType { get; set; }
    public string OrderName { get; set; } = string.Empty;
    public MedicalOrderStatus Status { get; set; } = MedicalOrderStatus.Ordered;
    public DateTime OrderedAt { get; set; } = DateTime.UtcNow;

    public Visit? Visit { get; set; }
    public List<MedicalResult> Results { get; set; } = [];
}

public sealed class MedicalResult : Entity<long>
{
    public long MedicalOrderId { get; set; }
    public string ResultText { get; set; } = string.Empty;
    public DateTime ResultDate { get; set; } = DateTime.UtcNow;
    public long? EnteredByStaffMemberId { get; set; }

    public MedicalOrder? MedicalOrder { get; set; }
    public StaffMember? EnteredByStaffMember { get; set; }
}

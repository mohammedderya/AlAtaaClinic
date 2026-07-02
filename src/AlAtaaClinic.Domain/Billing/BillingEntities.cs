using AlAtaaClinic.Domain.Clinical;
using AlAtaaClinic.Domain.Common;
using AlAtaaClinic.Domain.Core;
using AlAtaaClinic.Domain.Enums;

namespace AlAtaaClinic.Domain.Billing;

public sealed class Invoice : AggregateRoot
{
    public long BranchId { get; set; }
    public long PatientId { get; set; }
    public long? VisitId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;
    public decimal GrossAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal NetAmount { get; set; }

    public Branch? Branch { get; set; }
    public Patient? Patient { get; set; }
    public Visit? Visit { get; set; }
    public List<InvoiceLine> Lines { get; set; } = [];
    public List<Payment> Payments { get; set; } = [];
    public List<DiscountApproval> DiscountApprovals { get; set; } = [];
}

public sealed class InvoiceLine : Entity<long>
{
    public long InvoiceId { get; set; }
    public string? ServiceCode { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }

    public Invoice? Invoice { get; set; }
}

public sealed class Payment : Entity<long>
{
    public long InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public DateTime PaidAt { get; set; } = DateTime.UtcNow;
    public long? ReceivedByStaffMemberId { get; set; }
    public string? Notes { get; set; }

    public Invoice? Invoice { get; set; }
    public StaffMember? ReceivedByStaffMember { get; set; }
}

public sealed class DiscountApproval : Entity<long>
{
    public long InvoiceId { get; set; }
    public long? RequestedByStaffMemberId { get; set; }
    public long? ApprovedByStaffMemberId { get; set; }
    public decimal DiscountAmount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DiscountStatus Status { get; set; } = DiscountStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Invoice? Invoice { get; set; }
    public StaffMember? RequestedByStaffMember { get; set; }
    public StaffMember? ApprovedByStaffMember { get; set; }
}

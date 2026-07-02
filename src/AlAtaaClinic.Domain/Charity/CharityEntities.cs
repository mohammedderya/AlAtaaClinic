using AlAtaaClinic.Domain.Billing;
using AlAtaaClinic.Domain.Common;
using AlAtaaClinic.Domain.Core;
using AlAtaaClinic.Domain.Enums;

namespace AlAtaaClinic.Domain.Charity;

public sealed class CharityCase : AggregateRoot
{
    public long PatientId { get; set; }
    public string CaseNumber { get; set; } = string.Empty;
    public EligibilityStatus EligibilityStatus { get; set; } = EligibilityStatus.Pending;
    public decimal? CoveragePercentage { get; set; }
    public DateOnly ValidFrom { get; set; }
    public DateOnly? ValidTo { get; set; }
    public string? Notes { get; set; }

    public Patient? Patient { get; set; }
    public List<CharityCaseInvoice> Invoices { get; set; } = [];
}

public sealed class CharityCaseInvoice
{
    public long CharityCaseId { get; set; }
    public long InvoiceId { get; set; }
    public decimal CoveredAmount { get; set; }

    public CharityCase? CharityCase { get; set; }
    public Invoice? Invoice { get; set; }
}

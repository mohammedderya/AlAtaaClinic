using AlAtaaClinic.Domain.Enums;

namespace AlAtaaClinic.Application.Features.Invoices;

public sealed record InvoiceLineDto(
    string? ServiceCode,
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal LineTotal);

public sealed record PaymentDto(
    decimal Amount,
    PaymentMethod PaymentMethod,
    DateTime PaidAt,
    long? ReceivedByStaffMemberId,
    string? Notes);

public sealed record DiscountApprovalDto(
    long? RequestedByStaffMemberId,
    long? ApprovedByStaffMemberId,
    decimal DiscountAmount,
    string Reason,
    DiscountStatus Status);

public sealed record InvoiceDto(
    long Id,
    long BranchId,
    long PatientId,
    long? VisitId,
    string InvoiceNumber,
    InvoiceStatus Status,
    decimal GrossAmount,
    decimal DiscountAmount,
    decimal NetAmount,
    IReadOnlyList<InvoiceLineDto> Lines,
    IReadOnlyList<PaymentDto> Payments,
    IReadOnlyList<DiscountApprovalDto> DiscountApprovals);

public sealed record InvoiceListDto(
    long Id,
    string InvoiceNumber,
    long PatientId,
    InvoiceStatus Status,
    decimal NetAmount,
    DateTime CreatedAt);

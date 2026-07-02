using AlAtaaClinic.Application.Common.Messaging;
using AlAtaaClinic.Application.Common.Models;
using AlAtaaClinic.Domain.Enums;

namespace AlAtaaClinic.Application.Features.Invoices;

public sealed record CreateInvoiceCommand(
    long BranchId,
    long PatientId,
    long? VisitId,
    string InvoiceNumber,
    decimal DiscountAmount,
    IReadOnlyList<InvoiceLineDto> Lines) : ICommand<InvoiceDto>;

public sealed record UpdateInvoiceCommand(
    long Id,
    InvoiceStatus Status,
    decimal DiscountAmount,
    IReadOnlyList<InvoiceLineDto> Lines) : ICommand<InvoiceDto>;

public sealed record AddPaymentCommand(
    long InvoiceId,
    decimal Amount,
    PaymentMethod PaymentMethod,
    long? ReceivedByStaffMemberId,
    string? Notes) : ICommand<InvoiceDto>;

public sealed record DeleteInvoiceCommand(long Id) : ICommand<OperationResult>;

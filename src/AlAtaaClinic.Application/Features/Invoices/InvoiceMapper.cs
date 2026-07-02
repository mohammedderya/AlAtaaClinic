using AlAtaaClinic.Domain.Billing;

namespace AlAtaaClinic.Application.Features.Invoices;

public static class InvoiceMapper
{
    public static InvoiceDto ToDto(this Invoice invoice)
    {
        return new InvoiceDto(
            invoice.Id,
            invoice.BranchId,
            invoice.PatientId,
            invoice.VisitId,
            invoice.InvoiceNumber,
            invoice.Status,
            invoice.GrossAmount,
            invoice.DiscountAmount,
            invoice.NetAmount,
            invoice.Lines.Select(ToDto).ToList(),
            invoice.Payments.Select(ToDto).ToList(),
            invoice.DiscountApprovals.Select(ToDto).ToList());
    }

    public static InvoiceListDto ToListDto(this Invoice invoice)
    {
        return new InvoiceListDto(
            invoice.Id,
            invoice.InvoiceNumber,
            invoice.PatientId,
            invoice.Status,
            invoice.NetAmount,
            invoice.CreatedAt);
    }

    public static InvoiceLine ToEntity(this InvoiceLineDto dto)
    {
        return new InvoiceLine
        {
            ServiceCode = dto.ServiceCode,
            Description = dto.Description,
            Quantity = dto.Quantity,
            UnitPrice = dto.UnitPrice,
            LineTotal = dto.LineTotal
        };
    }

    private static InvoiceLineDto ToDto(InvoiceLine line)
    {
        return new InvoiceLineDto(line.ServiceCode, line.Description, line.Quantity, line.UnitPrice, line.LineTotal);
    }

    private static PaymentDto ToDto(Payment payment)
    {
        return new PaymentDto(payment.Amount, payment.PaymentMethod, payment.PaidAt, payment.ReceivedByStaffMemberId, payment.Notes);
    }

    private static DiscountApprovalDto ToDto(DiscountApproval approval)
    {
        return new DiscountApprovalDto(
            approval.RequestedByStaffMemberId,
            approval.ApprovedByStaffMemberId,
            approval.DiscountAmount,
            approval.Reason,
            approval.Status);
    }
}

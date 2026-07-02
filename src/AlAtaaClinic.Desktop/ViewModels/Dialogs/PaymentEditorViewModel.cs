using AlAtaaClinic.Application.Common.ExceptionHandling;
using AlAtaaClinic.Application.Features.Invoices;
using AlAtaaClinic.Desktop.Common;
using AlAtaaClinic.Domain.Enums;

namespace AlAtaaClinic.Desktop.ViewModels.Dialogs;

public sealed class PaymentEditorViewModel : DialogViewModelBase
{
    private readonly IInvoiceService _invoiceService;

    public PaymentEditorViewModel(IInvoiceService invoiceService, IExceptionHandler exceptionHandler, long invoiceId)
        : base(exceptionHandler)
    {
        _invoiceService = invoiceService;
        InvoiceId = invoiceId;
    }

    public long InvoiceId { get; }
    public PaymentMethod[] PaymentMethods { get; } = Enum.GetValues<PaymentMethod>();
    public decimal Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
    public long? ReceivedByStaffMemberId { get; set; }
    public string? Notes { get; set; }

    protected override Task SaveAsync()
    {
        return RunAsync(async () =>
        {
            await _invoiceService.AddPaymentAsync(new AddPaymentCommand(InvoiceId, Amount, PaymentMethod, ReceivedByStaffMemberId, Notes));
            Close(true);
        });
    }
}

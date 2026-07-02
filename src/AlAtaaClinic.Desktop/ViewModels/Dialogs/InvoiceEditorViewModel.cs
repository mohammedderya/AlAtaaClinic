using AlAtaaClinic.Application.Common.ExceptionHandling;
using AlAtaaClinic.Application.Features.Invoices;
using AlAtaaClinic.Desktop.Common;
using AlAtaaClinic.Domain.Enums;

namespace AlAtaaClinic.Desktop.ViewModels.Dialogs;

public sealed class InvoiceEditorViewModel : DialogViewModelBase
{
    private readonly IInvoiceService _invoiceService;

    public InvoiceEditorViewModel(IInvoiceService invoiceService, IExceptionHandler exceptionHandler, InvoiceDto? invoice = null)
        : base(exceptionHandler)
    {
        _invoiceService = invoiceService;
        Load(invoice);
    }

    public long Id { get; private set; }
    public bool IsEditMode => Id > 0;
    public InvoiceStatus[] Statuses { get; } = Enum.GetValues<InvoiceStatus>();
    public long BranchId { get; set; } = 1;
    public long PatientId { get; set; }
    public long? VisitId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;
    public decimal DiscountAmount { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal LineTotal => Quantity * UnitPrice;

    protected override Task SaveAsync()
    {
        return RunAsync(async () =>
        {
            if (IsEditMode)
            {
                await _invoiceService.UpdateAsync(ToUpdateCommand());
                Close(true);
                return;
            }

            await _invoiceService.CreateAsync(ToCreateCommand());
            Close(true);
        });
    }

    private CreateInvoiceCommand ToCreateCommand()
    {
        return new CreateInvoiceCommand(BranchId, PatientId, VisitId, InvoiceNumber, DiscountAmount, [ToLine()]);
    }

    private UpdateInvoiceCommand ToUpdateCommand()
    {
        return new UpdateInvoiceCommand(Id, Status, DiscountAmount, [ToLine()]);
    }

    private InvoiceLineDto ToLine()
    {
        return new InvoiceLineDto(null, Description, Quantity, UnitPrice, LineTotal);
    }

    private void Load(InvoiceDto? invoice)
    {
        if (invoice is null) return;
        var line = invoice.Lines.FirstOrDefault();
        Id = invoice.Id;
        BranchId = invoice.BranchId;
        PatientId = invoice.PatientId;
        VisitId = invoice.VisitId;
        InvoiceNumber = invoice.InvoiceNumber;
        Status = invoice.Status;
        DiscountAmount = invoice.DiscountAmount;
        Description = line?.Description ?? string.Empty;
        Quantity = line?.Quantity ?? 1;
        UnitPrice = line?.UnitPrice ?? 0;
    }
}

using System.Collections.ObjectModel;
using System.Windows.Input;
using AlAtaaClinic.Application.Common.ExceptionHandling;
using AlAtaaClinic.Application.Common.Models;
using AlAtaaClinic.Application.Features.Invoices;
using AlAtaaClinic.Desktop.Common;
using AlAtaaClinic.Desktop.Services;
using AlAtaaClinic.Desktop.ViewModels.Dialogs;

namespace AlAtaaClinic.Desktop.ViewModels;

public sealed class BillingViewModel : WorkspaceViewModelBase
{
    private readonly IInvoiceService _invoiceService;
    private readonly IExceptionHandler _exceptionHandler;
    private readonly IDialogService _dialogService;
    private readonly IPrintService _printService;
    private InvoiceListDto? _selectedInvoice;
    private InvoiceDto? _invoiceDetail;
    private long? _patientId;
    private string? _status;

    public BillingViewModel(
        IInvoiceService invoiceService,
        IExceptionHandler exceptionHandler,
        IDialogService dialogService,
        IPrintService printService)
        : base(exceptionHandler)
    {
        _invoiceService = invoiceService;
        _exceptionHandler = exceptionHandler;
        _dialogService = dialogService;
        _printService = printService;
        SearchCommand = new AsyncRelayCommand(_ => LoadAsync());
        AddCommand = new AsyncRelayCommand(_ => AddAsync());
        EditCommand = new AsyncRelayCommand(_ => EditAsync());
        LoadDetailCommand = new AsyncRelayCommand(_ => LoadDetailAsync());
        AddPaymentCommand = new AsyncRelayCommand(_ => AddPaymentAsync());
        VoidCommand = new AsyncRelayCommand(_ => VoidAsync());
        PrintCommand = new RelayCommand(_ => PrintSelected());
    }

    public ObservableCollection<InvoiceListDto> Invoices { get; } = [];
    public ICommand SearchCommand { get; }
    public ICommand AddCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand LoadDetailCommand { get; }
    public ICommand AddPaymentCommand { get; }
    public ICommand VoidCommand { get; }
    public ICommand PrintCommand { get; }

    public long? PatientId
    {
        get => _patientId;
        set => SetProperty(ref _patientId, value);
    }

    public string? Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }

    public InvoiceListDto? SelectedInvoice
    {
        get => _selectedInvoice;
        set
        {
            if (SetProperty(ref _selectedInvoice, value) && value is not null)
            {
                _ = LoadDetailAsync();
            }
        }
    }

    public InvoiceDto? InvoiceDetail
    {
        get => _invoiceDetail;
        set => SetProperty(ref _invoiceDetail, value);
    }

    public override Task InitializeAsync() => LoadAsync();

    private Task LoadAsync()
    {
        return RunAsync(async () =>
        {
            var result = await _invoiceService.SearchAsync(new SearchInvoicesQuery(PatientId, Status, new PageRequest(1, 100)));
            Invoices.Clear();
            foreach (var invoice in result.Items) Invoices.Add(invoice);
        });
    }

    private Task LoadDetailAsync()
    {
        if (SelectedInvoice is null) return Task.CompletedTask;
        return RunAsync(async () => InvoiceDetail = await _invoiceService.GetByIdAsync(new GetInvoiceByIdQuery(SelectedInvoice.Id)));
    }

    private async Task AddAsync()
    {
        var editor = new InvoiceEditorViewModel(_invoiceService, _exceptionHandler);
        if (_dialogService.ShowEditor(TranslationService.Default["Dialog.NewInvoice"], editor))
        {
            await LoadAsync();
        }
    }

    private async Task EditAsync()
    {
        if (SelectedInvoice is null) return;
        var invoice = await _invoiceService.GetByIdAsync(new GetInvoiceByIdQuery(SelectedInvoice.Id));
        var editor = new InvoiceEditorViewModel(_invoiceService, _exceptionHandler, invoice);
        if (_dialogService.ShowEditor(TranslationService.Default["Dialog.EditInvoice"], editor))
        {
            await LoadAsync();
            await LoadDetailAsync();
        }
    }

    private async Task AddPaymentAsync()
    {
        if (SelectedInvoice is null) return;
        var editor = new PaymentEditorViewModel(_invoiceService, _exceptionHandler, SelectedInvoice.Id);
        if (_dialogService.ShowEditor("Add Payment", editor))
        {
            await LoadDetailAsync();
            await LoadAsync();
        }
    }

    private Task VoidAsync()
    {
        if (SelectedInvoice is null || !_dialogService.Confirm("Void Invoice", "Void the selected invoice?"))
        {
            return Task.CompletedTask;
        }

        return RunAsync(async () =>
        {
            await _invoiceService.DeleteAsync(new DeleteInvoiceCommand(SelectedInvoice.Id));
            await LoadAsync();
        });
    }

    private void PrintSelected()
    {
        if (InvoiceDetail is null) return;
        var lines = new List<string>
        {
            $"Invoice: {InvoiceDetail.InvoiceNumber}",
            $"Patient: {InvoiceDetail.PatientId}",
            $"Status: {InvoiceDetail.Status}",
            $"Gross: {InvoiceDetail.GrossAmount:N2}",
            $"Discount: {InvoiceDetail.DiscountAmount:N2}",
            $"Net: {InvoiceDetail.NetAmount:N2}"
        };

        lines.AddRange(InvoiceDetail.Lines.Select(line => $"{line.Description}  {line.Quantity:N2} x {line.UnitPrice:N2} = {line.LineTotal:N2}"));
        _printService.PrintTextReport("Invoice", lines);
    }
}

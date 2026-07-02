using System.Collections.ObjectModel;
using System.Windows.Input;
using AlAtaaClinic.Application.Common.ExceptionHandling;
using AlAtaaClinic.Application.Common.Models;
using AlAtaaClinic.Application.Features.Appointments;
using AlAtaaClinic.Application.Features.Invoices;
using AlAtaaClinic.Application.Features.Patients;
using AlAtaaClinic.Application.Features.Visits;
using AlAtaaClinic.Desktop.Common;
using AlAtaaClinic.Desktop.Services;

namespace AlAtaaClinic.Desktop.ViewModels;

public sealed class ReportsViewModel : WorkspaceViewModelBase
{
    private readonly IPatientService _patientService;
    private readonly IAppointmentService _appointmentService;
    private readonly IVisitService _visitService;
    private readonly IInvoiceService _invoiceService;
    private readonly IPrintService _printService;

    public ReportsViewModel(
        IPatientService patientService,
        IAppointmentService appointmentService,
        IVisitService visitService,
        IInvoiceService invoiceService,
        IPrintService printService,
        IExceptionHandler exceptionHandler)
        : base(exceptionHandler)
    {
        _patientService = patientService;
        _appointmentService = appointmentService;
        _visitService = visitService;
        _invoiceService = invoiceService;
        _printService = printService;
        RefreshCommand = new AsyncRelayCommand(_ => InitializeAsync());
        PrintCommand = new RelayCommand(_ => Print());
    }

    public ObservableCollection<ChartItem> ChartItems { get; } = [];
    public ObservableCollection<string> ReportLines { get; } = [];
    public ICommand RefreshCommand { get; }
    public ICommand PrintCommand { get; }

    public override Task InitializeAsync()
    {
        return RunAsync(async () =>
        {
            var page = new PageRequest(1, 1);
            var patients = await _patientService.SearchAsync(new SearchPatientsQuery(null, page));
            var appointments = await _appointmentService.SearchAsync(new SearchAppointmentsQuery(null, DateTime.Today, DateTime.Today.AddDays(1), page));
            var visits = await _visitService.SearchAsync(new SearchVisitsQuery(null, null, DateTime.Today, DateTime.Today.AddDays(1), page));
            var invoices = await _invoiceService.SearchAsync(new SearchInvoicesQuery(null, null, page));
            UpdateReport(patients.TotalCount, appointments.TotalCount, visits.TotalCount, invoices.TotalCount);
        });
    }

    private void UpdateReport(int patients, int appointments, int visits, int invoices)
    {
        ReportLines.Clear();
        ReportLines.Add($"Total patients: {patients:N0}");
        ReportLines.Add($"Appointments today: {appointments:N0}");
        ReportLines.Add($"Clinical visits today: {visits:N0}");
        ReportLines.Add($"Invoices: {invoices:N0}");

        ChartItems.Clear();
        ChartItems.Add(new ChartItem("Patients", patients, Scale(patients)));
        ChartItems.Add(new ChartItem("Appointments", appointments, Scale(appointments)));
        ChartItems.Add(new ChartItem("Visits", visits, Scale(visits)));
        ChartItems.Add(new ChartItem("Invoices", invoices, Scale(invoices)));
    }

    private void Print()
    {
        _printService.PrintTextReport("Management Report", ReportLines.ToList());
    }

    private static double Scale(int value)
    {
        return Math.Max(24, Math.Min(320, value * 8));
    }
}

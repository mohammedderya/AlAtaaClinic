using System.Collections.ObjectModel;
using System.Windows.Input;
using AlAtaaClinic.Application.Common.ExceptionHandling;
using AlAtaaClinic.Application.Common.Models;
using AlAtaaClinic.Application.Features.Appointments;
using AlAtaaClinic.Application.Features.Invoices;
using AlAtaaClinic.Application.Features.Medicines;
using AlAtaaClinic.Application.Features.Patients;
using AlAtaaClinic.Application.Features.Visits;
using AlAtaaClinic.Desktop.Common;

namespace AlAtaaClinic.Desktop.ViewModels;

public sealed class DashboardViewModel : WorkspaceViewModelBase
{
    private readonly IPatientService _patientService;
    private readonly IAppointmentService _appointmentService;
    private readonly IVisitService _visitService;
    private readonly IInvoiceService _invoiceService;
    private readonly IMedicineService _medicineService;

    public DashboardViewModel(
        IPatientService patientService,
        IAppointmentService appointmentService,
        IVisitService visitService,
        IInvoiceService invoiceService,
        IMedicineService medicineService,
        IExceptionHandler exceptionHandler)
        : base(exceptionHandler)
    {
        _patientService = patientService;
        _appointmentService = appointmentService;
        _visitService = visitService;
        _invoiceService = invoiceService;
        _medicineService = medicineService;
        RefreshCommand = new AsyncRelayCommand(_ => InitializeAsync());
    }

    public ObservableCollection<MetricCardItem> Metrics { get; } = [];
    public ObservableCollection<ChartItem> ActivityChart { get; } = [];
    public ICommand RefreshCommand { get; }

    public override Task InitializeAsync()
    {
        return RunAsync(async () =>
        {
            var page = new PageRequest(1, 1);
            var patients = await _patientService.SearchAsync(new SearchPatientsQuery(null, page));
            var appointments = await _appointmentService.SearchAsync(new SearchAppointmentsQuery(null, DateTime.Today, DateTime.Today.AddDays(1), page));
            var visits = await _visitService.SearchAsync(new SearchVisitsQuery(null, null, DateTime.Today, DateTime.Today.AddDays(1), page));
            var invoices = await _invoiceService.SearchAsync(new SearchInvoicesQuery(null, null, page));
            var medicines = await _medicineService.SearchAsync(new SearchMedicinesQuery(null, page));

            UpdateMetrics(patients.TotalCount, appointments.TotalCount, visits.TotalCount, invoices.TotalCount, medicines.TotalCount);
        });
    }

    private void UpdateMetrics(int patients, int appointments, int visits, int invoices, int medicines)
    {
        Metrics.Clear();
        Metrics.Add(new MetricCardItem("Patients", patients.ToString("N0"), "Registered files", "Brush.Primary"));
        Metrics.Add(new MetricCardItem("Appointments", appointments.ToString("N0"), "Scheduled today", "Brush.Accent"));
        Metrics.Add(new MetricCardItem("Visits", visits.ToString("N0"), "Clinical encounters today", "Brush.Success"));
        Metrics.Add(new MetricCardItem("Invoices", invoices.ToString("N0"), "Financial records", "Brush.Warning"));
        Metrics.Add(new MetricCardItem("Medicines", medicines.ToString("N0"), "Catalog items", "Brush.Danger"));

        ActivityChart.Clear();
        ActivityChart.Add(new ChartItem("Patients", patients, Scale(patients)));
        ActivityChart.Add(new ChartItem("Appointments", appointments, Scale(appointments)));
        ActivityChart.Add(new ChartItem("Visits", visits, Scale(visits)));
        ActivityChart.Add(new ChartItem("Invoices", invoices, Scale(invoices)));
        ActivityChart.Add(new ChartItem("Medicines", medicines, Scale(medicines)));
    }

    private static double Scale(int value)
    {
        return Math.Max(24, Math.Min(320, value * 8));
    }
}

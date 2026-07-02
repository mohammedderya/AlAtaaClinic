using System.Collections.ObjectModel;
using System.Windows.Input;
using AlAtaaClinic.Application.Common.ExceptionHandling;
using AlAtaaClinic.Application.Common.Models;
using AlAtaaClinic.Application.Features.Appointments;
using AlAtaaClinic.Desktop.Common;
using AlAtaaClinic.Desktop.Services;
using AlAtaaClinic.Desktop.ViewModels.Dialogs;

namespace AlAtaaClinic.Desktop.ViewModels;

public sealed class AppointmentsViewModel : WorkspaceViewModelBase
{
    private readonly IAppointmentService _appointmentService;
    private readonly IExceptionHandler _exceptionHandler;
    private readonly IDialogService _dialogService;
    private readonly IPrintService _printService;
    private AppointmentListDto? _selectedAppointment;
    private DateTime? _from = DateTime.Today;
    private DateTime? _to = DateTime.Today.AddDays(1);
    private long? _doctorId;

    public AppointmentsViewModel(
        IAppointmentService appointmentService,
        IExceptionHandler exceptionHandler,
        IDialogService dialogService,
        IPrintService printService)
        : base(exceptionHandler)
    {
        _appointmentService = appointmentService;
        _exceptionHandler = exceptionHandler;
        _dialogService = dialogService;
        _printService = printService;
        SearchCommand = new AsyncRelayCommand(_ => LoadAsync());
        AddCommand = new AsyncRelayCommand(_ => AddAsync());
        EditCommand = new AsyncRelayCommand(_ => EditAsync());
        DeleteCommand = new AsyncRelayCommand(_ => DeleteAsync());
        PrintCommand = new RelayCommand(_ => Print());
    }

    public ObservableCollection<AppointmentListDto> Appointments { get; } = [];
    public ICommand SearchCommand { get; }
    public ICommand AddCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand PrintCommand { get; }

    public DateTime? From
    {
        get => _from;
        set => SetProperty(ref _from, value);
    }

    public DateTime? To
    {
        get => _to;
        set => SetProperty(ref _to, value);
    }

    public long? DoctorId
    {
        get => _doctorId;
        set => SetProperty(ref _doctorId, value);
    }

    public AppointmentListDto? SelectedAppointment
    {
        get => _selectedAppointment;
        set => SetProperty(ref _selectedAppointment, value);
    }

    public override Task InitializeAsync() => LoadAsync();

    private Task LoadAsync()
    {
        return RunAsync(async () =>
        {
            var result = await _appointmentService.SearchAsync(new SearchAppointmentsQuery(DoctorId, From, To, new PageRequest(1, 100)));
            Appointments.Clear();
            foreach (var appointment in result.Items) Appointments.Add(appointment);
        });
    }

    private async Task AddAsync()
    {
        var editor = new AppointmentEditorViewModel(_appointmentService, _exceptionHandler);
        if (_dialogService.ShowEditor(TranslationService.Default["Dialog.NewAppointment"], editor)) await LoadAsync();
    }

    private async Task EditAsync()
    {
        if (SelectedAppointment is null) return;
        var appointment = await _appointmentService.GetByIdAsync(new GetAppointmentByIdQuery(SelectedAppointment.Id));
        var editor = new AppointmentEditorViewModel(_appointmentService, _exceptionHandler, appointment);
        if (_dialogService.ShowEditor(TranslationService.Default["Dialog.EditAppointment"], editor)) await LoadAsync();
    }

    private Task DeleteAsync()
    {
        if (SelectedAppointment is null || !_dialogService.Confirm("Delete Appointment", "Delete the selected appointment?"))
        {
            return Task.CompletedTask;
        }

        return RunAsync(async () =>
        {
            await _appointmentService.DeleteAsync(new DeleteAppointmentCommand(SelectedAppointment.Id));
            await LoadAsync();
        });
    }

    private void Print()
    {
        var lines = Appointments.Select(item => $"{item.AppointmentStart:g}  Patient: {item.PatientId}  Doctor: {item.DoctorId}  {item.Status}");
        _printService.PrintTextReport("Appointments Report", lines.ToList());
    }
}

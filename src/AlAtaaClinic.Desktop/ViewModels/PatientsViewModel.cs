using System.Collections.ObjectModel;
using System.Windows.Input;
using AlAtaaClinic.Application.Common.ExceptionHandling;
using AlAtaaClinic.Application.Common.Models;
using AlAtaaClinic.Application.Features.Patients;
using AlAtaaClinic.Desktop.Common;
using AlAtaaClinic.Desktop.Services;
using AlAtaaClinic.Desktop.ViewModels.Dialogs;

namespace AlAtaaClinic.Desktop.ViewModels;

public sealed class PatientsViewModel : WorkspaceViewModelBase
{
    private readonly IPatientService _patientService;
    private readonly IExceptionHandler _exceptionHandler;
    private readonly IDialogService _dialogService;
    private readonly IPrintService _printService;
    private string? _searchText;
    private PatientListDto? _selectedPatient;

    public PatientsViewModel(
        IPatientService patientService,
        IExceptionHandler exceptionHandler,
        IDialogService dialogService,
        IPrintService printService)
        : base(exceptionHandler)
    {
        _patientService = patientService;
        _exceptionHandler = exceptionHandler;
        _dialogService = dialogService;
        _printService = printService;
        SearchCommand = new AsyncRelayCommand(_ => LoadAsync());
        AddCommand = new AsyncRelayCommand(_ => AddAsync());
        EditCommand = new AsyncRelayCommand(_ => EditAsync());
        DeleteCommand = new AsyncRelayCommand(_ => DeleteAsync());
        PrintCommand = new RelayCommand(_ => Print());
    }

    public ObservableCollection<PatientListDto> Patients { get; } = [];
    public ICommand SearchCommand { get; }
    public ICommand AddCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand PrintCommand { get; }

    public string? SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    public PatientListDto? SelectedPatient
    {
        get => _selectedPatient;
        set => SetProperty(ref _selectedPatient, value);
    }

    public override Task InitializeAsync() => LoadAsync();

    private Task LoadAsync()
    {
        return RunAsync(async () =>
        {
            var result = await _patientService.SearchAsync(new SearchPatientsQuery(SearchText, new PageRequest(1, 100)));
            Patients.Clear();
            foreach (var patient in result.Items)
            {
                Patients.Add(patient);
            }
        });
    }

    private async Task AddAsync()
    {
        var editor = new PatientEditorViewModel(_patientService, _exceptionHandler);
        if (_dialogService.ShowEditor(TranslationService.Default["Dialog.NewPatient"], editor))
        {
            await LoadAsync();
        }
    }

    private async Task EditAsync()
    {
        if (SelectedPatient is null)
        {
            return;
        }

        await RunAsync(async () =>
        {
            var patient = await _patientService.GetByIdAsync(new GetPatientByIdQuery(SelectedPatient.Id));
            var editor = new PatientEditorViewModel(_patientService, _exceptionHandler, patient);
            if (_dialogService.ShowEditor(TranslationService.Default["Dialog.EditPatient"], editor))
            {
                await LoadAsync();
            }
        });
    }

    private Task DeleteAsync()
    {
        if (SelectedPatient is null || !_dialogService.Confirm("Deactivate Patient", "Deactivate the selected patient file?"))
        {
            return Task.CompletedTask;
        }

        return RunAsync(async () =>
        {
            await _patientService.DeleteAsync(new DeletePatientCommand(SelectedPatient.Id));
            await LoadAsync();
        });
    }

    private void Print()
    {
        var lines = Patients.Select(patient => $"{patient.FileNumber}  {patient.FullNameArabic}  {patient.PhoneNumber}");
        _printService.PrintTextReport("Patients Report", lines.ToList());
    }
}

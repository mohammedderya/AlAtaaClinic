using System.Collections.ObjectModel;
using System.Windows.Input;
using AlAtaaClinic.Application.Common.ExceptionHandling;
using AlAtaaClinic.Application.Common.Models;
using AlAtaaClinic.Application.Features.Branches;
using AlAtaaClinic.Application.Features.Doctors;
using AlAtaaClinic.Application.Features.Patients;
using AlAtaaClinic.Application.Features.Visits;
using AlAtaaClinic.Desktop.Common;
using AlAtaaClinic.Desktop.Services;
using AlAtaaClinic.Desktop.ViewModels.Dialogs;

namespace AlAtaaClinic.Desktop.ViewModels;

public sealed class VisitsViewModel : WorkspaceViewModelBase
{
    private readonly IVisitService _visitService;
    private readonly IBranchService _branchService;
    private readonly IPatientService _patientService;
    private readonly IDoctorService _doctorService;
    private readonly IExceptionHandler _exceptionHandler;
    private readonly IDialogService _dialogService;
    private readonly IPrintService _printService;
    private VisitListDto? _selectedVisit;
    private VisitDto? _visitDetail;
    private long? _patientId;
    private long? _doctorId;
    private DateTime? _from = DateTime.Today;
    private DateTime? _to = DateTime.Today.AddDays(1);

    public VisitsViewModel(
        IVisitService visitService,
        IBranchService branchService,
        IPatientService patientService,
        IDoctorService doctorService,
        IDialogService dialogService,
        IPrintService printService,
        IExceptionHandler exceptionHandler)
        : base(exceptionHandler)
    {
        _visitService = visitService;
        _branchService = branchService;
        _patientService = patientService;
        _doctorService = doctorService;
        _exceptionHandler = exceptionHandler;
        _dialogService = dialogService;
        _printService = printService;
        SearchCommand = new AsyncRelayCommand(_ => LoadAsync());
        AddCommand = new AsyncRelayCommand(_ => AddAsync());
        EditCommand = new AsyncRelayCommand(_ => EditAsync());
        CancelCommand = new AsyncRelayCommand(_ => CancelAsync());
        LoadDetailCommand = new AsyncRelayCommand(_ => LoadDetailAsync());
        PrintCommand = new RelayCommand(_ => PrintSelected());
    }

    public ObservableCollection<VisitListDto> Visits { get; } = [];
    public ICommand SearchCommand { get; }
    public ICommand AddCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand LoadDetailCommand { get; }
    public ICommand PrintCommand { get; }

    public long? PatientId
    {
        get => _patientId;
        set => SetProperty(ref _patientId, value);
    }

    public long? DoctorId
    {
        get => _doctorId;
        set => SetProperty(ref _doctorId, value);
    }

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

    public VisitListDto? SelectedVisit
    {
        get => _selectedVisit;
        set
        {
            if (SetProperty(ref _selectedVisit, value) && value is not null)
            {
                _ = LoadDetailAsync();
            }
        }
    }

    public VisitDto? VisitDetail
    {
        get => _visitDetail;
        set => SetProperty(ref _visitDetail, value);
    }

    public override Task InitializeAsync() => LoadAsync();

    private Task LoadAsync()
    {
        return RunAsync(async () =>
        {
            var result = await _visitService.SearchAsync(new SearchVisitsQuery(PatientId, DoctorId, From, To, new PageRequest(1, 100)));
            Visits.Clear();
            foreach (var visit in result.Items) Visits.Add(visit);
        });
    }

    private Task LoadDetailAsync()
    {
        if (SelectedVisit is null)
        {
            return Task.CompletedTask;
        }

        return RunAsync(async () => VisitDetail = await _visitService.GetByIdAsync(new GetVisitByIdQuery(SelectedVisit.Id)));
    }

    private async Task AddAsync()
    {
        var editor = new VisitEditorViewModel(_visitService, _exceptionHandler);
        var branches = await _branchService.SearchAsync(new SearchBranchesQuery(null, new PageRequest(1, 200)));
        var patients = await _patientService.SearchAsync(new SearchPatientsQuery(null, new PageRequest(1, 200)));
        var doctors = await _doctorService.SearchAsync(new SearchDoctorsQuery(null, new PageRequest(1, 200)));
        editor.SetLookups(branches.Items, patients.Items, doctors.Items);
        if (_dialogService.ShowEditor(TranslationService.Default["Dialog.NewVisit"], editor))
        {
            await LoadAsync();
        }
    }

    private async Task EditAsync()
    {
        if (SelectedVisit is null) return;
        var visit = await _visitService.GetByIdAsync(new GetVisitByIdQuery(SelectedVisit.Id));
        var editor = new VisitEditorViewModel(_visitService, _exceptionHandler, visit);
        var branches = await _branchService.SearchAsync(new SearchBranchesQuery(null, new PageRequest(1, 200)));
        var patients = await _patientService.SearchAsync(new SearchPatientsQuery(null, new PageRequest(1, 200)));
        var doctors = await _doctorService.SearchAsync(new SearchDoctorsQuery(null, new PageRequest(1, 200)));
        editor.SetLookups(branches.Items, patients.Items, doctors.Items);
        if (_dialogService.ShowEditor(TranslationService.Default["Dialog.EditVisit"], editor))
        {
            await LoadAsync();
            VisitDetail = null;
        }
    }

    private Task CancelAsync()
    {
        if (SelectedVisit is null || !_dialogService.Confirm("Cancel Visit", "Cancel the selected clinical visit?"))
        {
            return Task.CompletedTask;
        }

        return RunAsync(async () =>
        {
            await _visitService.DeleteAsync(new DeleteVisitCommand(SelectedVisit.Id));
            await LoadAsync();
            VisitDetail = null;
        });
    }

    private void PrintSelected()
    {
        if (VisitDetail is null)
        {
            return;
        }

        var lines = new List<string>
        {
            $"Visit: {VisitDetail.Id}",
            $"Patient: {VisitDetail.PatientId}",
            $"Doctor: {VisitDetail.DoctorId}",
            $"Date: {VisitDetail.VisitDate:g}",
            $"Complaint: {VisitDetail.ChiefComplaint}",
            $"Notes: {VisitDetail.ClinicalNotes}"
        };

        lines.AddRange(VisitDetail.Diagnoses.Select(diagnosis => $"Diagnosis: {diagnosis.Description}"));
        _printService.PrintTextReport("Clinical Visit", lines);
    }
}

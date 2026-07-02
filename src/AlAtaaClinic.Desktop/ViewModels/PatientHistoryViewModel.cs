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

public sealed class PatientHistoryViewModel : WorkspaceViewModelBase
{
    private readonly IVisitService _visitService;
    private readonly IPatientService _patientService;
    private readonly IBranchService _branchService;
    private readonly IDoctorService _doctorService;
    private readonly IDialogService _dialogService;
    private PatientListDto? _selectedPatient;
    private VisitListDto? _selectedVisit;
    private VisitDto? _visitDetail;
    private string _searchText = string.Empty;

    public PatientHistoryViewModel(
        IVisitService visitService,
        IPatientService patientService,
        IBranchService branchService,
        IDoctorService doctorService,
        IDialogService dialogService,
        IExceptionHandler exceptionHandler)
        : base(exceptionHandler)
    {
        _visitService = visitService;
        _patientService = patientService;
        _branchService = branchService;
        _doctorService = doctorService;
        _dialogService = dialogService;
        SearchPatientCommand = new AsyncRelayCommand(_ => SearchPatientAsync());
        AddVisitCommand = new AsyncRelayCommand(_ => AddVisitAsync());
        EditVisitCommand = new AsyncRelayCommand(_ => EditVisitAsync());
    }

    public ObservableCollection<PatientListDto> Patients { get; } = [];
    public ObservableCollection<VisitListDto> Visits { get; } = [];
    public ICommand SearchPatientCommand { get; }
    public ICommand AddVisitCommand { get; }
    public ICommand EditVisitCommand { get; }

    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    public PatientListDto? SelectedPatient
    {
        get => _selectedPatient;
        set
        {
            if (SetProperty(ref _selectedPatient, value) && value is not null)
            {
                _ = LoadVisitsAsync();
            }
        }
    }

    public VisitListDto? SelectedVisit
    {
        get => _selectedVisit;
        set
        {
            if (SetProperty(ref _selectedVisit, value) && value is not null)
            {
                _ = LoadDetailAsync(value.Id);
            }
        }
    }

    public VisitDto? VisitDetail
    {
        get => _visitDetail;
        set => SetProperty(ref _visitDetail, value);
    }

    private async Task SearchPatientAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchText)) return;
        await RunAsync(async () =>
        {
            var result = await _patientService.SearchAsync(
                new SearchPatientsQuery(SearchText.Trim(), new PageRequest(1, 50)));
            Patients.Clear();
            foreach (var p in result.Items) Patients.Add(p);
        });
    }

    private async Task LoadVisitsAsync()
    {
        if (SelectedPatient is null) return;
        await RunAsync(async () =>
        {
            var result = await _visitService.SearchAsync(
                new SearchVisitsQuery(SelectedPatient.Id, null, null, null, new PageRequest(1, 200)));
            Visits.Clear();
            foreach (var v in result.Items) Visits.Add(v);
            VisitDetail = null;
        });
    }

    private Task LoadDetailAsync(long visitId)
    {
        return RunAsync(async () =>
            VisitDetail = await _visitService.GetByIdAsync(new GetVisitByIdQuery(visitId)));
    }

    private async Task AddVisitAsync()
    {
        if (SelectedPatient is null)
        {
            _dialogService.ShowInfo("Select Patient", "Please select a patient first.");
            return;
        }

        var editor = new VisitEditorViewModel(_visitService, ExceptionHandler);
        var branches = await _branchService.SearchAsync(new SearchBranchesQuery(null, new PageRequest(1, 200)));
        var patients = await _patientService.SearchAsync(new SearchPatientsQuery(null, new PageRequest(1, 200)));
        var doctors = await _doctorService.SearchAsync(new SearchDoctorsQuery(null, new PageRequest(1, 200)));
        editor.SetLookups(branches.Items, patients.Items, doctors.Items);
        editor.SelectPatient(SelectedPatient.Id);

        if (_dialogService.ShowEditor(TranslationService.Default["Dialog.NewVisit"], editor))
        {
            await LoadVisitsAsync();
        }
    }

    private async Task EditVisitAsync()
    {
        if (SelectedVisit is null) return;
        var visit = await _visitService.GetByIdAsync(new GetVisitByIdQuery(SelectedVisit.Id));
        var editor = new VisitEditorViewModel(_visitService, ExceptionHandler, visit);
        var branches = await _branchService.SearchAsync(new SearchBranchesQuery(null, new PageRequest(1, 200)));
        var patients = await _patientService.SearchAsync(new SearchPatientsQuery(null, new PageRequest(1, 200)));
        var doctors = await _doctorService.SearchAsync(new SearchDoctorsQuery(null, new PageRequest(1, 200)));
        editor.SetLookups(branches.Items, patients.Items, doctors.Items);
        if (_dialogService.ShowEditor(TranslationService.Default["Dialog.EditVisit"], editor))
        {
            await LoadVisitsAsync();
            VisitDetail = null;
        }
    }
}

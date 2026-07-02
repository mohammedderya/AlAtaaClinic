using System.Collections.ObjectModel;
using System.Windows.Input;
using AlAtaaClinic.Application.Common.ExceptionHandling;
using AlAtaaClinic.Application.Common.Models;
using AlAtaaClinic.Application.Features.CharityCases;
using AlAtaaClinic.Desktop.Common;
using AlAtaaClinic.Desktop.Services;
using AlAtaaClinic.Desktop.ViewModels.Dialogs;

namespace AlAtaaClinic.Desktop.ViewModels;

public sealed class CharityCasesViewModel : WorkspaceViewModelBase
{
    private readonly ICharityCaseService _charityCaseService;
    private readonly IExceptionHandler _exceptionHandler;
    private readonly IDialogService _dialogService;
    private readonly IPrintService _printService;
    private long? _patientId;
    private string? _caseNumber;
    private CharityCaseListDto? _selectedCharityCase;

    public CharityCasesViewModel(
        ICharityCaseService charityCaseService,
        IDialogService dialogService,
        IPrintService printService,
        IExceptionHandler exceptionHandler)
        : base(exceptionHandler)
    {
        _charityCaseService = charityCaseService;
        _exceptionHandler = exceptionHandler;
        _dialogService = dialogService;
        _printService = printService;
        SearchCommand = new AsyncRelayCommand(_ => LoadAsync());
        AddCommand = new AsyncRelayCommand(_ => AddAsync());
        EditCommand = new AsyncRelayCommand(_ => EditAsync());
        DeleteCommand = new AsyncRelayCommand(_ => DeleteAsync());
        PrintCommand = new RelayCommand(_ => Print());
    }

    public ObservableCollection<CharityCaseListDto> CharityCases { get; } = [];
    public ICommand SearchCommand { get; }
    public ICommand AddCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand PrintCommand { get; }

    public long? PatientId
    {
        get => _patientId;
        set => SetProperty(ref _patientId, value);
    }

    public string? CaseNumber
    {
        get => _caseNumber;
        set => SetProperty(ref _caseNumber, value);
    }

    public CharityCaseListDto? SelectedCharityCase
    {
        get => _selectedCharityCase;
        set => SetProperty(ref _selectedCharityCase, value);
    }

    public override Task InitializeAsync() => LoadAsync();

    private Task LoadAsync()
    {
        return RunAsync(async () =>
        {
            var result = await _charityCaseService.SearchAsync(new SearchCharityCasesQuery(PatientId, CaseNumber, new PageRequest(1, 100)));
            CharityCases.Clear();
            foreach (var charityCase in result.Items) CharityCases.Add(charityCase);
        });
    }

    private async Task AddAsync()
    {
        var editor = new CharityCaseEditorViewModel(_charityCaseService, _exceptionHandler);
        if (_dialogService.ShowEditor(TranslationService.Default["Dialog.NewCharityCase"], editor))
        {
            await LoadAsync();
        }
    }

    private async Task EditAsync()
    {
        if (SelectedCharityCase is null) return;
        var charityCase = await _charityCaseService.GetByIdAsync(new GetCharityCaseByIdQuery(SelectedCharityCase.Id));
        var editor = new CharityCaseEditorViewModel(_charityCaseService, _exceptionHandler, charityCase);
        if (_dialogService.ShowEditor(TranslationService.Default["Dialog.EditCharityCase"], editor))
        {
            await LoadAsync();
        }
    }

    private Task DeleteAsync()
    {
        if (SelectedCharityCase is null || !_dialogService.Confirm("Suspend Charity Case", "Suspend the selected charity case?"))
        {
            return Task.CompletedTask;
        }

        return RunAsync(async () =>
        {
            await _charityCaseService.DeleteAsync(new DeleteCharityCaseCommand(SelectedCharityCase.Id));
            await LoadAsync();
        });
    }

    private void Print()
    {
        var lines = CharityCases.Select(item => $"{item.CaseNumber}  Patient: {item.PatientId}  {item.EligibilityStatus}  Coverage: {item.CoveragePercentage:N2}%");
        _printService.PrintTextReport("Charity Cases Report", lines.ToList());
    }
}

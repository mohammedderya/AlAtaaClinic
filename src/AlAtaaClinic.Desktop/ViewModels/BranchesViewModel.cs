using System.Collections.ObjectModel;
using System.Windows.Input;
using AlAtaaClinic.Application.Common.ExceptionHandling;
using AlAtaaClinic.Application.Common.Models;
using AlAtaaClinic.Application.Features.Branches;
using AlAtaaClinic.Desktop.Common;
using AlAtaaClinic.Desktop.Services;
using AlAtaaClinic.Desktop.ViewModels.Dialogs;

namespace AlAtaaClinic.Desktop.ViewModels;

public sealed class BranchesViewModel : WorkspaceViewModelBase
{
    private readonly IBranchService _branchService;
    private readonly IExceptionHandler _exceptionHandler;
    private readonly IDialogService _dialogService;
    private readonly IPrintService _printService;
    private string? _searchText;
    private BranchListDto? _selectedBranch;

    public BranchesViewModel(
        IBranchService branchService,
        IExceptionHandler exceptionHandler,
        IDialogService dialogService,
        IPrintService printService)
        : base(exceptionHandler)
    {
        _branchService = branchService;
        _exceptionHandler = exceptionHandler;
        _dialogService = dialogService;
        _printService = printService;
        SearchCommand = new AsyncRelayCommand(_ => LoadAsync());
        AddCommand = new AsyncRelayCommand(_ => AddAsync());
        EditCommand = new AsyncRelayCommand(_ => EditAsync());
        DeleteCommand = new AsyncRelayCommand(_ => DeleteAsync());
        PrintCommand = new RelayCommand(_ => Print());
    }

    public ObservableCollection<BranchListDto> Branches { get; } = [];
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

    public BranchListDto? SelectedBranch
    {
        get => _selectedBranch;
        set => SetProperty(ref _selectedBranch, value);
    }

    public override Task InitializeAsync() => LoadAsync();

    private Task LoadAsync()
    {
        return RunAsync(async () =>
        {
            var result = await _branchService.SearchAsync(new SearchBranchesQuery(SearchText, new PageRequest(1, 100)));
            Branches.Clear();
            foreach (var branch in result.Items)
            {
                Branches.Add(branch);
            }
        });
    }

    private async Task AddAsync()
    {
        var editor = new BranchEditorViewModel(_branchService, _exceptionHandler);
        if (_dialogService.ShowEditor(TranslationService.Default["Dialog.NewBranch"], editor))
        {
            await LoadAsync();
        }
    }

    private async Task EditAsync()
    {
        if (SelectedBranch is null)
        {
            return;
        }

        await RunAsync(async () =>
        {
            var branch = await _branchService.GetByIdAsync(new GetBranchByIdQuery(SelectedBranch.Id));
            var editor = new BranchEditorViewModel(_branchService, _exceptionHandler, branch);
            if (_dialogService.ShowEditor(TranslationService.Default["Dialog.EditBranch"], editor))
            {
                await LoadAsync();
            }
        });
    }

    private Task DeleteAsync()
    {
        if (SelectedBranch is null || !_dialogService.Confirm("Deactivate Branch", "Deactivate the selected branch?"))
        {
            return Task.CompletedTask;
        }

        return RunAsync(async () =>
        {
            await _branchService.DeleteAsync(new DeleteBranchCommand(SelectedBranch.Id));
            await LoadAsync();
        });
    }

    private void Print()
    {
        var lines = Branches.Select(branch => $"{branch.BranchCode}  {branch.ArabicName}  {branch.PhoneNumber}");
        _printService.PrintTextReport("Branches Report", lines.ToList());
    }
}

using System.Collections.ObjectModel;
using System.Windows.Input;
using AlAtaaClinic.Application.Common.ExceptionHandling;
using AlAtaaClinic.Application.Common.Models;
using AlAtaaClinic.Application.Features.Branches;
using AlAtaaClinic.Application.Features.Departments;
using AlAtaaClinic.Desktop.Common;
using AlAtaaClinic.Desktop.Services;
using AlAtaaClinic.Desktop.ViewModels.Dialogs;

namespace AlAtaaClinic.Desktop.ViewModels;

public sealed class DepartmentsViewModel : WorkspaceViewModelBase
{
    private readonly IDepartmentService _departmentService;
    private readonly IBranchService _branchService;
    private readonly IExceptionHandler _exceptionHandler;
    private readonly IDialogService _dialogService;
    private readonly IPrintService _printService;
    private string? _searchText;
    private BranchListDto? _selectedBranch;
    private DepartmentListDto? _selectedDepartment;

    public DepartmentsViewModel(
        IDepartmentService departmentService,
        IBranchService branchService,
        IExceptionHandler exceptionHandler,
        IDialogService dialogService,
        IPrintService printService)
        : base(exceptionHandler)
    {
        _departmentService = departmentService;
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

    public ObservableCollection<DepartmentListDto> Departments { get; } = [];
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
        set
        {
            if (SetProperty(ref _selectedBranch, value))
            {
                _ = LoadAsync();
            }
        }
    }

    public DepartmentListDto? SelectedDepartment
    {
        get => _selectedDepartment;
        set => SetProperty(ref _selectedDepartment, value);
    }

    public override async Task InitializeAsync()
    {
        await LoadBranchesAsync();
        await LoadAsync();
    }

    private async Task LoadBranchesAsync()
    {
        await RunAsync(async () =>
        {
            var result = await _branchService.SearchAsync(new SearchBranchesQuery(null, new PageRequest(1, 200)));
            Branches.Clear();
            foreach (var branch in result.Items)
            {
                Branches.Add(branch);
            }
        });
    }

    private Task LoadAsync()
    {
        return RunAsync(async () =>
        {
            var result = await _departmentService.SearchAsync(new SearchDepartmentsQuery(SearchText, SelectedBranch?.Id, new PageRequest(1, 100)));
            Departments.Clear();
            foreach (var dept in result.Items)
            {
                Departments.Add(dept);
            }
        });
    }

    private async Task AddAsync()
    {
        var editor = new DepartmentEditorViewModel(_departmentService, _exceptionHandler, Branches);
        if (_dialogService.ShowEditor(TranslationService.Default["Dialog.NewDepartment"], editor))
        {
            await LoadAsync();
        }
    }

    private async Task EditAsync()
    {
        if (SelectedDepartment is null)
        {
            return;
        }

        await RunAsync(async () =>
        {
            var dept = await _departmentService.GetByIdAsync(new GetDepartmentByIdQuery(SelectedDepartment.Id));
            var editor = new DepartmentEditorViewModel(_departmentService, _exceptionHandler, Branches, dept);
            if (_dialogService.ShowEditor(TranslationService.Default["Dialog.EditDepartment"], editor))
            {
                await LoadAsync();
            }
        });
    }

    private Task DeleteAsync()
    {
        if (SelectedDepartment is null || !_dialogService.Confirm("Deactivate Department", "Deactivate the selected department?"))
        {
            return Task.CompletedTask;
        }

        return RunAsync(async () =>
        {
            await _departmentService.DeleteAsync(new DeleteDepartmentCommand(SelectedDepartment.Id));
            await LoadAsync();
        });
    }

    private void Print()
    {
        var lines = Departments.Select(dept => $"{dept.ArabicName}  {dept.BranchName}");
        _printService.PrintTextReport("Departments Report", lines.ToList());
    }
}

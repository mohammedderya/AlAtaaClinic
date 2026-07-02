using System.Collections.ObjectModel;
using System.Windows.Input;
using AlAtaaClinic.Application.Common.ExceptionHandling;
using AlAtaaClinic.Application.Common.Models;
using AlAtaaClinic.Application.Features.Branches;
using AlAtaaClinic.Application.Features.Departments;
using AlAtaaClinic.Application.Features.StaffMembers;
using AlAtaaClinic.Desktop.Common;
using AlAtaaClinic.Desktop.Services;
using AlAtaaClinic.Desktop.ViewModels.Dialogs;

namespace AlAtaaClinic.Desktop.ViewModels;

public sealed class StaffMembersViewModel : WorkspaceViewModelBase
{
    private readonly IStaffMemberService _staffMemberService;
    private readonly IBranchService _branchService;
    private readonly IDepartmentService _departmentService;
    private readonly IDialogService _dialogService;
    private readonly IPrintService _printService;
    private string? _searchText;
    private StaffMemberListDto? _selectedStaffMember;

    public StaffMembersViewModel(
        IStaffMemberService staffMemberService,
        IBranchService branchService,
        IDepartmentService departmentService,
        IExceptionHandler exceptionHandler,
        IDialogService dialogService,
        IPrintService printService)
        : base(exceptionHandler)
    {
        _staffMemberService = staffMemberService;
        _branchService = branchService;
        _departmentService = departmentService;
        _dialogService = dialogService;
        _printService = printService;
        SearchCommand = new AsyncRelayCommand(_ => LoadAsync());
        AddCommand = new AsyncRelayCommand(_ => AddAsync());
        EditCommand = new AsyncRelayCommand(_ => EditAsync());
        DeleteCommand = new AsyncRelayCommand(_ => DeleteAsync());
        PrintCommand = new RelayCommand(_ => Print());
    }

    public ObservableCollection<StaffMemberListDto> StaffMembers { get; } = [];
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

    public StaffMemberListDto? SelectedStaffMember
    {
        get => _selectedStaffMember;
        set => SetProperty(ref _selectedStaffMember, value);
    }

    public override Task InitializeAsync() => LoadAsync();

    private Task LoadAsync()
    {
        return RunAsync(async () =>
        {
            var result = await _staffMemberService.SearchAsync(new SearchStaffMembersQuery(SearchText, new PageRequest(1, 100)));
            StaffMembers.Clear();
            foreach (var item in result.Items)
            {
                StaffMembers.Add(item);
            }
        });
    }

    private async Task AddAsync()
    {
        await RunAsync(async () =>
        {
            var branches = (await _branchService.SearchAsync(new SearchBranchesQuery(null, new PageRequest(1, 200)))).Items;
            var departments = (await _departmentService.SearchAsync(new SearchDepartmentsQuery(null, null, new PageRequest(1, 200)))).Items;
            var editor = new StaffMemberEditorViewModel(_staffMemberService, ExceptionHandler);
            editor.SetLookups(branches, departments);
            if (_dialogService.ShowEditor(TranslationService.Default["Dialog.NewStaffMember"], editor))
            {
                await LoadAsync();
            }
        });
    }

    private async Task EditAsync()
    {
        if (SelectedStaffMember is null)
        {
            return;
        }

        await RunAsync(async () =>
        {
            var staffMember = await _staffMemberService.GetByIdAsync(new GetStaffMemberByIdQuery(SelectedStaffMember.Id));
            var branches = (await _branchService.SearchAsync(new SearchBranchesQuery(null, new PageRequest(1, 200)))).Items;
            var departments = (await _departmentService.SearchAsync(new SearchDepartmentsQuery(null, null, new PageRequest(1, 200)))).Items;
            var editor = new StaffMemberEditorViewModel(_staffMemberService, ExceptionHandler, staffMember);
            editor.SetLookups(branches, departments);
            if (_dialogService.ShowEditor(TranslationService.Default["Dialog.EditStaffMember"], editor))
            {
                await LoadAsync();
            }
        });
    }

    private Task DeleteAsync()
    {
        if (SelectedStaffMember is null || !_dialogService.Confirm("Deactivate Staff Member", "Deactivate the selected staff member?"))
        {
            return Task.CompletedTask;
        }

        return RunAsync(async () =>
        {
            await _staffMemberService.DeleteAsync(new DeleteStaffMemberCommand(SelectedStaffMember.Id));
            await LoadAsync();
        });
    }

    private void Print()
    {
        var lines = StaffMembers.Select(s => $"{s.StaffCode}  {s.FullNameArabic}  {s.JobTitle}  {s.PhoneNumber}");
        _printService.PrintTextReport("Staff Members Report", lines.ToList());
    }
}

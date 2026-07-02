using System.Collections.ObjectModel;
using System.Windows.Input;
using AlAtaaClinic.Application.Common.ExceptionHandling;
using AlAtaaClinic.Application.Common.Models;
using AlAtaaClinic.Application.Features.Doctors;
using AlAtaaClinic.Application.Features.StaffMembers;
using AlAtaaClinic.Desktop.Common;
using AlAtaaClinic.Desktop.Services;
using AlAtaaClinic.Desktop.ViewModels.Dialogs;

namespace AlAtaaClinic.Desktop.ViewModels;

public sealed class DoctorsViewModel : WorkspaceViewModelBase
{
    private readonly IDoctorService _doctorService;
    private readonly IStaffMemberService _staffMemberService;
    private readonly IDialogService _dialogService;
    private readonly IPrintService _printService;
    private string? _searchText;
    private DoctorListDto? _selectedDoctor;

    public DoctorsViewModel(
        IDoctorService doctorService,
        IStaffMemberService staffMemberService,
        IExceptionHandler exceptionHandler,
        IDialogService dialogService,
        IPrintService printService)
        : base(exceptionHandler)
    {
        _doctorService = doctorService;
        _staffMemberService = staffMemberService;
        _dialogService = dialogService;
        _printService = printService;
        SearchCommand = new AsyncRelayCommand(_ => LoadAsync());
        AddCommand = new AsyncRelayCommand(_ => AddAsync());
        EditCommand = new AsyncRelayCommand(_ => EditAsync());
        DeleteCommand = new AsyncRelayCommand(_ => DeleteAsync());
        PrintCommand = new RelayCommand(_ => Print());
    }

    public ObservableCollection<DoctorListDto> Doctors { get; } = [];
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

    public DoctorListDto? SelectedDoctor
    {
        get => _selectedDoctor;
        set => SetProperty(ref _selectedDoctor, value);
    }

    public override Task InitializeAsync() => LoadAsync();

    private Task LoadAsync()
    {
        return RunAsync(async () =>
        {
            var result = await _doctorService.SearchAsync(new SearchDoctorsQuery(SearchText, new PageRequest(1, 100)));
            Doctors.Clear();
            foreach (var item in result.Items)
            {
                Doctors.Add(item);
            }
        });
    }

    private async Task AddAsync()
    {
        await RunAsync(async () =>
        {
            var staffMembers = await _staffMemberService.SearchAsync(new SearchStaffMembersQuery(null, new PageRequest(1, 200)));
            var editor = new DoctorEditorViewModel(_doctorService, _staffMemberService, ExceptionHandler);
            editor.SetStaffMembers(staffMembers.Items);
            if (_dialogService.ShowEditor(TranslationService.Default["Dialog.NewDoctor"], editor))
            {
                await LoadAsync();
            }
        });
    }

    private async Task EditAsync()
    {
        if (SelectedDoctor is null)
        {
            return;
        }

        await RunAsync(async () =>
        {
            var doctor = await _doctorService.GetByIdAsync(new GetDoctorByIdQuery(SelectedDoctor.Id));
            var staffMembers = await _staffMemberService.SearchAsync(new SearchStaffMembersQuery(null, new PageRequest(1, 200)));
            var editor = new DoctorEditorViewModel(_doctorService, _staffMemberService, ExceptionHandler, doctor);
            editor.SetStaffMembers(staffMembers.Items);
            if (_dialogService.ShowEditor(TranslationService.Default["Dialog.EditDoctor"], editor))
            {
                await LoadAsync();
            }
        });
    }

    private Task DeleteAsync()
    {
        if (SelectedDoctor is null || !_dialogService.Confirm("Deactivate Doctor", "Deactivate the selected doctor?"))
        {
            return Task.CompletedTask;
        }

        return RunAsync(async () =>
        {
            await _doctorService.DeleteAsync(new DeleteDoctorCommand(SelectedDoctor.Id));
            await LoadAsync();
        });
    }

    private void Print()
    {
        var lines = Doctors.Select(d => $"{d.StaffMemberName}  {d.Specialty}  {d.LicenseNumber}");
        _printService.PrintTextReport("Doctors Report", lines.ToList());
    }
}

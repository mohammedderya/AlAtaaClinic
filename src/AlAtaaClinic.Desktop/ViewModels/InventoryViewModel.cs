using System.Collections.ObjectModel;
using System.Windows.Input;
using AlAtaaClinic.Application.Common.ExceptionHandling;
using AlAtaaClinic.Application.Common.Models;
using AlAtaaClinic.Application.Features.Medicines;
using AlAtaaClinic.Desktop.Common;
using AlAtaaClinic.Desktop.Services;
using AlAtaaClinic.Desktop.ViewModels.Dialogs;

namespace AlAtaaClinic.Desktop.ViewModels;

public sealed class InventoryViewModel : WorkspaceViewModelBase
{
    private readonly IMedicineService _medicineService;
    private readonly IExceptionHandler _exceptionHandler;
    private readonly IDialogService _dialogService;
    private readonly IPrintService _printService;
    private string? _searchText;
    private MedicineListDto? _selectedMedicine;

    public InventoryViewModel(
        IMedicineService medicineService,
        IExceptionHandler exceptionHandler,
        IDialogService dialogService,
        IPrintService printService)
        : base(exceptionHandler)
    {
        _medicineService = medicineService;
        _exceptionHandler = exceptionHandler;
        _dialogService = dialogService;
        _printService = printService;
        SearchCommand = new AsyncRelayCommand(_ => LoadAsync());
        AddCommand = new AsyncRelayCommand(_ => AddAsync());
        EditCommand = new AsyncRelayCommand(_ => EditAsync());
        DeleteCommand = new AsyncRelayCommand(_ => DeleteAsync());
        PrintCommand = new RelayCommand(_ => Print());
    }

    public ObservableCollection<MedicineListDto> Medicines { get; } = [];
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

    public MedicineListDto? SelectedMedicine
    {
        get => _selectedMedicine;
        set => SetProperty(ref _selectedMedicine, value);
    }

    public override Task InitializeAsync() => LoadAsync();

    private Task LoadAsync()
    {
        return RunAsync(async () =>
        {
            var result = await _medicineService.SearchAsync(new SearchMedicinesQuery(SearchText, new PageRequest(1, 100)));
            Medicines.Clear();
            foreach (var medicine in result.Items) Medicines.Add(medicine);
        });
    }

    private async Task AddAsync()
    {
        var editor = new MedicineEditorViewModel(_medicineService, _exceptionHandler);
        if (_dialogService.ShowEditor(TranslationService.Default["Dialog.NewMedicine"], editor)) await LoadAsync();
    }

    private async Task EditAsync()
    {
        if (SelectedMedicine is null) return;
        var medicine = await _medicineService.GetByIdAsync(new GetMedicineByIdQuery(SelectedMedicine.Id));
        var editor = new MedicineEditorViewModel(_medicineService, _exceptionHandler, medicine);
        if (_dialogService.ShowEditor(TranslationService.Default["Dialog.EditMedicine"], editor)) await LoadAsync();
    }

    private Task DeleteAsync()
    {
        if (SelectedMedicine is null || !_dialogService.Confirm("Deactivate Medicine", "Deactivate the selected medicine?"))
        {
            return Task.CompletedTask;
        }

        return RunAsync(async () =>
        {
            await _medicineService.DeleteAsync(new DeleteMedicineCommand(SelectedMedicine.Id));
            await LoadAsync();
        });
    }

    private void Print()
    {
        var lines = Medicines.Select(item => $"{item.MedicineCode}  {item.GenericName}  {item.Unit}  Active: {item.IsActive}");
        _printService.PrintTextReport("Inventory Report", lines.ToList());
    }
}

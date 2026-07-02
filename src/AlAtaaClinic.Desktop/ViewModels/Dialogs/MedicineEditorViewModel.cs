using AlAtaaClinic.Application.Common.ExceptionHandling;
using AlAtaaClinic.Application.Features.Medicines;
using AlAtaaClinic.Desktop.Common;

namespace AlAtaaClinic.Desktop.ViewModels.Dialogs;

public sealed class MedicineEditorViewModel : DialogViewModelBase
{
    private readonly IMedicineService _medicineService;

    public MedicineEditorViewModel(IMedicineService medicineService, IExceptionHandler exceptionHandler, MedicineDto? medicine = null)
        : base(exceptionHandler)
    {
        _medicineService = medicineService;
        Load(medicine);
    }

    public long Id { get; private set; }
    public bool IsEditMode => Id > 0;
    public string MedicineCode { get; set; } = string.Empty;
    public string GenericName { get; set; } = string.Empty;
    public string? TradeName { get; set; }
    public string Unit { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    protected override Task SaveAsync()
    {
        return RunAsync(async () =>
        {
            if (IsEditMode)
            {
                await _medicineService.UpdateAsync(new UpdateMedicineCommand(Id, GenericName, TradeName, Unit, IsActive));
                Close(true);
                return;
            }

            await _medicineService.CreateAsync(new CreateMedicineCommand(MedicineCode, GenericName, TradeName, Unit));
            Close(true);
        });
    }

    private void Load(MedicineDto? medicine)
    {
        if (medicine is null)
        {
            return;
        }

        Id = medicine.Id;
        MedicineCode = medicine.MedicineCode;
        GenericName = medicine.GenericName;
        TradeName = medicine.TradeName;
        Unit = medicine.Unit;
        IsActive = medicine.IsActive;
    }
}

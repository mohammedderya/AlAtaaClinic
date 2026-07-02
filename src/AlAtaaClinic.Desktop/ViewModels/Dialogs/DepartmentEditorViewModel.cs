using System.Collections.ObjectModel;
using AlAtaaClinic.Application.Common.ExceptionHandling;
using AlAtaaClinic.Application.Features.Branches;
using AlAtaaClinic.Application.Features.Departments;
using AlAtaaClinic.Desktop.Common;

namespace AlAtaaClinic.Desktop.ViewModels.Dialogs;

public sealed class DepartmentEditorViewModel : DialogViewModelBase
{
    private readonly IDepartmentService _departmentService;

    public DepartmentEditorViewModel(
        IDepartmentService departmentService,
        IExceptionHandler exceptionHandler,
        IEnumerable<BranchListDto> branches,
        DepartmentDto? department = null)
        : base(exceptionHandler)
    {
        _departmentService = departmentService;
        foreach (var branch in branches)
        {
            Branches.Add(branch);
        }
        Load(department);
    }

    public long Id { get; private set; }
    public bool IsEditMode => Id > 0;
    public long BranchId { get; set; }
    public string ArabicName { get; set; } = string.Empty;
    public string? EnglishName { get; set; }
    public bool IsActive { get; set; } = true;
    public ObservableCollection<BranchListDto> Branches { get; } = [];

    protected override Task SaveAsync()
    {
        return RunAsync(async () =>
        {
            if (IsEditMode)
            {
                await UpdateAsync();
                Close(true);
                return;
            }

            await CreateAsync();
            Close(true);
        });
    }

    private async Task CreateAsync()
    {
        await _departmentService.CreateAsync(new CreateDepartmentCommand(
            BranchId,
            ArabicName,
            EnglishName));
    }

    private async Task UpdateAsync()
    {
        await _departmentService.UpdateAsync(new UpdateDepartmentCommand(
            Id,
            BranchId,
            ArabicName,
            EnglishName,
            IsActive));
    }

    private void Load(DepartmentDto? department)
    {
        if (department is null)
        {
            return;
        }

        Id = department.Id;
        BranchId = department.BranchId;
        ArabicName = department.ArabicName;
        EnglishName = department.EnglishName;
        IsActive = department.IsActive;
    }
}

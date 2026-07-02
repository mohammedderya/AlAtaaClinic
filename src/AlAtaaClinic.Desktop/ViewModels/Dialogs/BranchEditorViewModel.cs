using AlAtaaClinic.Application.Common.ExceptionHandling;
using AlAtaaClinic.Application.Features.Branches;
using AlAtaaClinic.Desktop.Common;

namespace AlAtaaClinic.Desktop.ViewModels.Dialogs;

public sealed class BranchEditorViewModel : DialogViewModelBase
{
    private readonly IBranchService _branchService;

    public BranchEditorViewModel(IBranchService branchService, IExceptionHandler exceptionHandler, BranchDto? branch = null)
        : base(exceptionHandler)
    {
        _branchService = branchService;
        Load(branch);
    }

    public long Id { get; private set; }
    public bool IsEditMode => Id > 0;
    public string BranchCode { get; set; } = string.Empty;
    public string ArabicName { get; set; } = string.Empty;
    public string? EnglishName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? AddressLine { get; set; }
    public bool IsActive { get; set; } = true;

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
        await _branchService.CreateAsync(new CreateBranchCommand(
            BranchCode,
            ArabicName,
            EnglishName,
            PhoneNumber,
            AddressLine));
    }

    private async Task UpdateAsync()
    {
        await _branchService.UpdateAsync(new UpdateBranchCommand(
            Id,
            ArabicName,
            EnglishName,
            PhoneNumber,
            AddressLine,
            IsActive));
    }

    private void Load(BranchDto? branch)
    {
        if (branch is null)
        {
            return;
        }

        Id = branch.Id;
        BranchCode = branch.BranchCode;
        ArabicName = branch.ArabicName;
        EnglishName = branch.EnglishName;
        PhoneNumber = branch.PhoneNumber;
        AddressLine = branch.AddressLine;
        IsActive = branch.IsActive;
    }
}

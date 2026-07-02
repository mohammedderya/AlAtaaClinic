using AlAtaaClinic.Application.Common.ExceptionHandling;
using AlAtaaClinic.Application.Features.CharityCases;
using AlAtaaClinic.Desktop.Common;
using AlAtaaClinic.Domain.Enums;

namespace AlAtaaClinic.Desktop.ViewModels.Dialogs;

public sealed class CharityCaseEditorViewModel : DialogViewModelBase
{
    private readonly ICharityCaseService _charityCaseService;

    public CharityCaseEditorViewModel(ICharityCaseService charityCaseService, IExceptionHandler exceptionHandler, CharityCaseDto? charityCase = null)
        : base(exceptionHandler)
    {
        _charityCaseService = charityCaseService;
        ValidFrom = DateTime.Today;
        Load(charityCase);
    }

    public long Id { get; private set; }
    public bool IsEditMode => Id > 0;
    public EligibilityStatus[] Statuses { get; } = Enum.GetValues<EligibilityStatus>();
    public long PatientId { get; set; }
    public string CaseNumber { get; set; } = string.Empty;
    public EligibilityStatus EligibilityStatus { get; set; } = EligibilityStatus.Pending;
    public decimal? CoveragePercentage { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public string? Notes { get; set; }

    protected override Task SaveAsync()
    {
        return RunAsync(async () =>
        {
            if (IsEditMode)
            {
                await _charityCaseService.UpdateAsync(ToUpdateCommand());
                Close(true);
                return;
            }

            await _charityCaseService.CreateAsync(ToCreateCommand());
            Close(true);
        });
    }

    private CreateCharityCaseCommand ToCreateCommand()
    {
        return new CreateCharityCaseCommand(
            PatientId,
            CaseNumber,
            EligibilityStatus,
            CoveragePercentage,
            DateOnly.FromDateTime(ValidFrom),
            ToDateOnly(ValidTo),
            Notes);
    }

    private UpdateCharityCaseCommand ToUpdateCommand()
    {
        return new UpdateCharityCaseCommand(
            Id,
            EligibilityStatus,
            CoveragePercentage,
            DateOnly.FromDateTime(ValidFrom),
            ToDateOnly(ValidTo),
            Notes);
    }

    private void Load(CharityCaseDto? charityCase)
    {
        if (charityCase is null) return;
        Id = charityCase.Id;
        PatientId = charityCase.PatientId;
        CaseNumber = charityCase.CaseNumber;
        EligibilityStatus = charityCase.EligibilityStatus;
        CoveragePercentage = charityCase.CoveragePercentage;
        ValidFrom = charityCase.ValidFrom.ToDateTime(TimeOnly.MinValue);
        ValidTo = charityCase.ValidTo?.ToDateTime(TimeOnly.MinValue);
        Notes = charityCase.Notes;
    }

    private static DateOnly? ToDateOnly(DateTime? value)
    {
        return value.HasValue ? DateOnly.FromDateTime(value.Value) : null;
    }
}

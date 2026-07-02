using AlAtaaClinic.Application.Common.ExceptionHandling;
using AlAtaaClinic.Application.Features.Patients;
using AlAtaaClinic.Desktop.Common;
using AlAtaaClinic.Domain.Enums;

namespace AlAtaaClinic.Desktop.ViewModels.Dialogs;

public sealed class PatientEditorViewModel : DialogViewModelBase
{
    private readonly IPatientService _patientService;

    public PatientEditorViewModel(IPatientService patientService, IExceptionHandler exceptionHandler, PatientDto? patient = null)
        : base(exceptionHandler)
    {
        _patientService = patientService;
        Load(patient);
    }

    public long Id { get; private set; }
    public bool IsEditMode => Id > 0;
    public Gender[] Genders { get; } = Enum.GetValues<Gender>();
    public string FileNumber { get; set; } = string.Empty;
    public string? NationalId { get; set; }
    public string FullNameArabic { get; set; } = string.Empty;
    public string? FullNameEnglish { get; set; }
    public Gender Gender { get; set; } = Gender.Male;
    public DateTime? DateOfBirth { get; set; }
    public string? PhoneNumber { get; set; }
    public string? AlternativePhoneNumber { get; set; }
    public string? AddressLine { get; set; }
    public string? BloodType { get; set; }
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
        await _patientService.CreateAsync(new CreatePatientCommand(
            FileNumber,
            NationalId,
            FullNameArabic,
            FullNameEnglish,
            Gender,
            ToDateOnly(DateOfBirth),
            PhoneNumber,
            AlternativePhoneNumber,
            AddressLine,
            BloodType));
    }

    private async Task UpdateAsync()
    {
        await _patientService.UpdateAsync(new UpdatePatientCommand(
            Id,
            NationalId,
            FullNameArabic,
            FullNameEnglish,
            Gender,
            ToDateOnly(DateOfBirth),
            PhoneNumber,
            AlternativePhoneNumber,
            AddressLine,
            BloodType,
            IsActive));
    }

    private void Load(PatientDto? patient)
    {
        if (patient is null)
        {
            return;
        }

        Id = patient.Id;
        FileNumber = patient.FileNumber;
        NationalId = patient.NationalId;
        FullNameArabic = patient.FullNameArabic;
        FullNameEnglish = patient.FullNameEnglish;
        Gender = patient.Gender;
        DateOfBirth = patient.DateOfBirth?.ToDateTime(TimeOnly.MinValue);
        PhoneNumber = patient.PhoneNumber;
        AlternativePhoneNumber = patient.AlternativePhoneNumber;
        AddressLine = patient.AddressLine;
        BloodType = patient.BloodType;
        IsActive = patient.IsActive;
    }

    private static DateOnly? ToDateOnly(DateTime? value)
    {
        return value.HasValue ? DateOnly.FromDateTime(value.Value) : null;
    }
}

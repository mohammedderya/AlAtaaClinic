using System.Collections.ObjectModel;
using AlAtaaClinic.Application.Common.ExceptionHandling;
using AlAtaaClinic.Application.Features.Doctors;
using AlAtaaClinic.Application.Features.StaffMembers;
using AlAtaaClinic.Desktop.Common;

namespace AlAtaaClinic.Desktop.ViewModels.Dialogs;

public sealed class DoctorEditorViewModel : DialogViewModelBase
{
    private readonly IDoctorService _doctorService;
    private readonly IStaffMemberService _staffMemberService;

    public DoctorEditorViewModel(
        IDoctorService doctorService,
        IStaffMemberService staffMemberService,
        IExceptionHandler exceptionHandler,
        DoctorDto? doctor = null)
        : base(exceptionHandler)
    {
        _doctorService = doctorService;
        _staffMemberService = staffMemberService;
        Load(doctor);
    }

    public long Id { get; private set; }
    public bool IsEditMode => Id > 0;
    public string Specialty { get; set; } = string.Empty;
    public string? LicenseNumber { get; set; }
    public bool IsActive { get; set; } = true;

    public ObservableCollection<StaffMemberListDto> StaffMembers { get; } = [];
    public StaffMemberListDto? SelectedStaffMember { get; set; }

    public long StaffMemberId => SelectedStaffMember?.Id ?? 0;

    private long _originalStaffMemberId;

    public void SetStaffMembers(IReadOnlyList<StaffMemberListDto> staffMembers)
    {
        StaffMembers.Clear();
        foreach (var s in staffMembers)
        {
            StaffMembers.Add(s);
        }

        if (IsEditMode)
        {
            SelectedStaffMember = StaffMembers.FirstOrDefault(s => s.Id == _originalStaffMemberId);
        }
    }

    protected override async Task SaveAsync()
    {
        await RunAsync(async () =>
        {
            if (StaffMemberId == 0)
            {
                ValidationMessages.Add("Please select a staff member.");
                return;
            }

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
        await _doctorService.CreateAsync(new CreateDoctorCommand(
            StaffMemberId,
            Specialty,
            LicenseNumber));
    }

    private async Task UpdateAsync()
    {
        await _doctorService.UpdateAsync(new UpdateDoctorCommand(
            Id,
            Specialty,
            LicenseNumber,
            IsActive));
    }

    private void Load(DoctorDto? doctor)
    {
        if (doctor is null)
        {
            return;
        }

        Id = doctor.Id;
        Specialty = doctor.Specialty;
        LicenseNumber = doctor.LicenseNumber;
        IsActive = doctor.IsActive;
        _originalStaffMemberId = doctor.StaffMemberId;
    }
}

using System.Collections.ObjectModel;
using AlAtaaClinic.Application.Common.ExceptionHandling;
using AlAtaaClinic.Application.Features.Branches;
using AlAtaaClinic.Application.Features.Departments;
using AlAtaaClinic.Application.Features.StaffMembers;
using AlAtaaClinic.Desktop.Common;

namespace AlAtaaClinic.Desktop.ViewModels.Dialogs;

public sealed class StaffMemberEditorViewModel : DialogViewModelBase
{
    private readonly IStaffMemberService _staffMemberService;

    public StaffMemberEditorViewModel(
        IStaffMemberService staffMemberService,
        IExceptionHandler exceptionHandler,
        StaffMemberDto? staffMember = null)
        : base(exceptionHandler)
    {
        _staffMemberService = staffMemberService;
        Load(staffMember);
    }

    public long Id { get; private set; }
    public bool IsEditMode => Id > 0;
    public string StaffCode { get; set; } = string.Empty;
    public string FullNameArabic { get; set; } = string.Empty;
    public string? FullNameEnglish { get; set; }
    public string? PhoneNumber { get; set; }
    public string? JobTitle { get; set; }
    public bool IsActive { get; set; } = true;

    public ObservableCollection<BranchListDto> Branches { get; } = [];
    public BranchListDto? SelectedBranch { get; set; }

    public ObservableCollection<DepartmentListDto> Departments { get; } = [];
    public DepartmentListDto? SelectedDepartment { get; set; }

    public long BranchId => SelectedBranch?.Id ?? 0;
    public long? DepartmentId => SelectedDepartment?.Id;

    public void SetLookups(IReadOnlyList<BranchListDto> branches, IReadOnlyList<DepartmentListDto> departments)
    {
        Branches.Clear();
        foreach (var b in branches) Branches.Add(b);

        Departments.Clear();
        foreach (var d in departments) Departments.Add(d);

        if (Id > 0)
        {
            SelectedBranch = Branches.FirstOrDefault(b => b.Id == _originalBranchId);
            SelectedDepartment = _originalDepartmentId.HasValue
                ? Departments.FirstOrDefault(d => d.Id == _originalDepartmentId.Value)
                : null;
        }
    }

    private long _originalBranchId;
    private long? _originalDepartmentId;

    protected override async Task SaveAsync()
    {
        await RunAsync(async () =>
        {
            if (BranchId == 0)
            {
                ValidationMessages.Add("Please select a branch.");
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
        await _staffMemberService.CreateAsync(new CreateStaffMemberCommand(
            BranchId,
            DepartmentId,
            StaffCode,
            FullNameArabic,
            FullNameEnglish,
            PhoneNumber,
            JobTitle));
    }

    private async Task UpdateAsync()
    {
        await _staffMemberService.UpdateAsync(new UpdateStaffMemberCommand(
            Id,
            BranchId,
            DepartmentId,
            FullNameArabic,
            FullNameEnglish,
            PhoneNumber,
            JobTitle,
            IsActive));
    }

    private void Load(StaffMemberDto? staffMember)
    {
        if (staffMember is null)
        {
            return;
        }

        Id = staffMember.Id;
        StaffCode = staffMember.StaffCode;
        FullNameArabic = staffMember.FullNameArabic;
        FullNameEnglish = staffMember.FullNameEnglish;
        PhoneNumber = staffMember.PhoneNumber;
        JobTitle = staffMember.JobTitle;
        IsActive = staffMember.IsActive;
        _originalBranchId = staffMember.BranchId;
        _originalDepartmentId = staffMember.DepartmentId;
    }
}

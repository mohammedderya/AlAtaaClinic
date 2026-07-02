using AlAtaaClinic.Domain.Common;
using AlAtaaClinic.Domain.Enums;

namespace AlAtaaClinic.Domain.Core;

public sealed class Branch : AggregateRoot
{
    public string BranchCode { get; set; } = string.Empty;
    public string ArabicName { get; set; } = string.Empty;
    public string? EnglishName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? AddressLine { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class Department : AggregateRoot
{
    public long BranchId { get; set; }
    public string ArabicName { get; set; } = string.Empty;
    public string? EnglishName { get; set; }
    public bool IsActive { get; set; } = true;

    public Branch? Branch { get; set; }
}

public sealed class StaffMember : AggregateRoot
{
    public long BranchId { get; set; }
    public long? DepartmentId { get; set; }
    public string StaffCode { get; set; } = string.Empty;
    public string FullNameArabic { get; set; } = string.Empty;
    public string? FullNameEnglish { get; set; }
    public string? PhoneNumber { get; set; }
    public string? JobTitle { get; set; }
    public bool IsActive { get; set; } = true;

    public Branch? Branch { get; set; }
    public Department? Department { get; set; }
}

public sealed class Doctor : Entity<long>
{
    public long StaffMemberId { get; set; }
    public string Specialty { get; set; } = string.Empty;
    public string? LicenseNumber { get; set; }
    public bool IsActive { get; set; } = true;

    public StaffMember? StaffMember { get; set; }
}

public sealed class Patient : AggregateRoot
{
    public string FileNumber { get; set; } = string.Empty;
    public string? NationalId { get; set; }
    public string FullNameArabic { get; set; } = string.Empty;
    public string? FullNameEnglish { get; set; }
    public Gender Gender { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? PhoneNumber { get; set; }
    public string? AlternativePhoneNumber { get; set; }
    public string? AddressLine { get; set; }
    public string? BloodType { get; set; }
    public bool IsActive { get; set; } = true;
}

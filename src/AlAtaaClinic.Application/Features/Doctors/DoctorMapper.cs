using AlAtaaClinic.Domain.Core;

namespace AlAtaaClinic.Application.Features.Doctors;

public static class DoctorMapper
{
    public static DoctorDto ToDto(this Doctor doctor)
    {
        return new DoctorDto(
            doctor.Id,
            doctor.StaffMemberId,
            doctor.StaffMember?.FullNameArabic,
            doctor.StaffMember?.StaffCode,
            doctor.Specialty,
            doctor.LicenseNumber,
            doctor.IsActive);
    }

    public static DoctorListDto ToListDto(this Doctor doctor)
    {
        return new DoctorListDto(
            doctor.Id,
            doctor.StaffMember?.FullNameArabic,
            doctor.StaffMember?.StaffCode,
            doctor.Specialty,
            doctor.LicenseNumber,
            doctor.IsActive);
    }
}

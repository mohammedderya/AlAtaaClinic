using AlAtaaClinic.Domain.Core;

namespace AlAtaaClinic.Application.Features.Patients;

public static class PatientMapper
{
    public static PatientDto ToDto(this Patient patient)
    {
        return new PatientDto(
            patient.Id,
            patient.FileNumber,
            patient.NationalId,
            patient.FullNameArabic,
            patient.FullNameEnglish,
            patient.Gender,
            patient.DateOfBirth,
            patient.PhoneNumber,
            patient.AlternativePhoneNumber,
            patient.AddressLine,
            patient.BloodType,
            patient.IsActive);
    }

    public static PatientListDto ToListDto(this Patient patient)
    {
        return new PatientListDto(
            patient.Id,
            patient.FileNumber,
            patient.FullNameArabic,
            patient.PhoneNumber,
            patient.Gender,
            patient.IsActive);
    }
}

using AlAtaaClinic.Domain.Charity;

namespace AlAtaaClinic.Application.Features.CharityCases;

public static class CharityCaseMapper
{
    public static CharityCaseDto ToDto(this CharityCase charityCase)
    {
        return new CharityCaseDto(
            charityCase.Id,
            charityCase.PatientId,
            charityCase.CaseNumber,
            charityCase.EligibilityStatus,
            charityCase.CoveragePercentage,
            charityCase.ValidFrom,
            charityCase.ValidTo,
            charityCase.Notes);
    }

    public static CharityCaseListDto ToListDto(this CharityCase charityCase)
    {
        return new CharityCaseListDto(
            charityCase.Id,
            charityCase.PatientId,
            charityCase.CaseNumber,
            charityCase.EligibilityStatus,
            charityCase.CoveragePercentage);
    }
}

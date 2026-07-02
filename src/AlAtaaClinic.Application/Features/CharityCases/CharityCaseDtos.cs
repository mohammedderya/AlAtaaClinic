using AlAtaaClinic.Domain.Enums;

namespace AlAtaaClinic.Application.Features.CharityCases;

public sealed record CharityCaseDto(
    long Id,
    long PatientId,
    string CaseNumber,
    EligibilityStatus EligibilityStatus,
    decimal? CoveragePercentage,
    DateOnly ValidFrom,
    DateOnly? ValidTo,
    string? Notes);

public sealed record CharityCaseListDto(
    long Id,
    long PatientId,
    string CaseNumber,
    EligibilityStatus EligibilityStatus,
    decimal? CoveragePercentage);

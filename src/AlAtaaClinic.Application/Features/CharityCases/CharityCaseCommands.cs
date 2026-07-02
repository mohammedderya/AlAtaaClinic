using AlAtaaClinic.Application.Common.Messaging;
using AlAtaaClinic.Application.Common.Models;
using AlAtaaClinic.Domain.Enums;

namespace AlAtaaClinic.Application.Features.CharityCases;

public sealed record CreateCharityCaseCommand(
    long PatientId,
    string CaseNumber,
    EligibilityStatus EligibilityStatus,
    decimal? CoveragePercentage,
    DateOnly ValidFrom,
    DateOnly? ValidTo,
    string? Notes) : ICommand<CharityCaseDto>;

public sealed record UpdateCharityCaseCommand(
    long Id,
    EligibilityStatus EligibilityStatus,
    decimal? CoveragePercentage,
    DateOnly ValidFrom,
    DateOnly? ValidTo,
    string? Notes) : ICommand<CharityCaseDto>;

public sealed record DeleteCharityCaseCommand(long Id) : ICommand<OperationResult>;

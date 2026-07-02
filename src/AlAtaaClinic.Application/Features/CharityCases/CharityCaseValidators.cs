using AlAtaaClinic.Application.Common.Validation;

namespace AlAtaaClinic.Application.Features.CharityCases;

public sealed class CreateCharityCaseCommandValidator : ValidatorBase<CreateCharityCaseCommand>
{
    protected override void ValidateRules(CreateCharityCaseCommand instance)
    {
        if (instance.PatientId <= 0) Add(nameof(instance.PatientId), "Patient is required.");
        Required(instance.CaseNumber, nameof(instance.CaseNumber));
        ValidateCoverage(instance.CoveragePercentage);
        ValidateDates(instance.ValidFrom, instance.ValidTo);
    }

    private void ValidateCoverage(decimal? coveragePercentage)
    {
        if (coveragePercentage is < 0 or > 100)
        {
            Add(nameof(CreateCharityCaseCommand.CoveragePercentage), "Coverage must be between 0 and 100.");
        }
    }

    private void ValidateDates(DateOnly validFrom, DateOnly? validTo)
    {
        if (validTo < validFrom)
        {
            Add(nameof(CreateCharityCaseCommand.ValidTo), "Valid-to date must be after valid-from date.");
        }
    }
}

public sealed class UpdateCharityCaseCommandValidator : ValidatorBase<UpdateCharityCaseCommand>
{
    protected override void ValidateRules(UpdateCharityCaseCommand instance)
    {
        if (instance.Id <= 0) Add(nameof(instance.Id), "Charity case id is required.");
        if (instance.CoveragePercentage is < 0 or > 100)
        {
            Add(nameof(instance.CoveragePercentage), "Coverage must be between 0 and 100.");
        }

        if (instance.ValidTo < instance.ValidFrom)
        {
            Add(nameof(instance.ValidTo), "Valid-to date must be after valid-from date.");
        }
    }
}

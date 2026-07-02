using AlAtaaClinic.Application.Abstractions.Logging;
using AlAtaaClinic.Application.Abstractions.Persistence;
using AlAtaaClinic.Application.Abstractions.Security;
using AlAtaaClinic.Application.Common.Exceptions;
using AlAtaaClinic.Application.Common.Messaging;
using AlAtaaClinic.Application.Common.Models;
using AlAtaaClinic.Application.Common.Validation;
using AlAtaaClinic.Domain.Charity;
using AlAtaaClinic.Domain.Enums;

namespace AlAtaaClinic.Application.Features.CharityCases;

public sealed class CharityCaseService :
    ICharityCaseService,
    ICommandHandler<CreateCharityCaseCommand, CharityCaseDto>,
    ICommandHandler<UpdateCharityCaseCommand, CharityCaseDto>,
    ICommandHandler<DeleteCharityCaseCommand, OperationResult>,
    IQueryHandler<GetCharityCaseByIdQuery, CharityCaseDto>,
    IQueryHandler<SearchCharityCasesQuery, PagedResult<CharityCaseListDto>>
{
    private readonly ICharityCaseRepository _charityCases;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthorizationService _authorization;
    private readonly ValidationRunner<CreateCharityCaseCommand> _createValidator;
    private readonly ValidationRunner<UpdateCharityCaseCommand> _updateValidator;
    private readonly IAppLogger<CharityCaseService> _logger;

    public CharityCaseService(
        ICharityCaseRepository charityCases,
        IUnitOfWork unitOfWork,
        IAuthorizationService authorization,
        ValidationRunner<CreateCharityCaseCommand> createValidator,
        ValidationRunner<UpdateCharityCaseCommand> updateValidator,
        IAppLogger<CharityCaseService> logger)
    {
        _charityCases = charityCases;
        _unitOfWork = unitOfWork;
        _authorization = authorization;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _logger = logger;
    }

    public async Task<CharityCaseDto> CreateAsync(CreateCharityCaseCommand command, CancellationToken cancellationToken = default)
    {
        await _authorization.EnsurePermissionAsync(PermissionKeys.CharityWrite, cancellationToken);
        _createValidator.ValidateAndThrow(command);

        var charityCase = CreateEntity(command);
        await _charityCases.AddAsync(charityCase, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.Information($"Charity case '{charityCase.CaseNumber}' created.");
        return charityCase.ToDto();
    }

    public async Task<CharityCaseDto> UpdateAsync(UpdateCharityCaseCommand command, CancellationToken cancellationToken = default)
    {
        await _authorization.EnsurePermissionAsync(PermissionKeys.CharityWrite, cancellationToken);
        _updateValidator.ValidateAndThrow(command);

        var charityCase = await GetCaseOrThrowAsync(command.Id, cancellationToken);
        Apply(command, charityCase);
        _charityCases.Update(charityCase);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return charityCase.ToDto();
    }

    public async Task<OperationResult> DeleteAsync(DeleteCharityCaseCommand command, CancellationToken cancellationToken = default)
    {
        await _authorization.EnsurePermissionAsync(PermissionKeys.CharityWrite, cancellationToken);
        var charityCase = await GetCaseOrThrowAsync(command.Id, cancellationToken);
        charityCase.EligibilityStatus = EligibilityStatus.Suspended;
        charityCase.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult.Success("Charity case suspended.");
    }

    public async Task<CharityCaseDto> GetByIdAsync(GetCharityCaseByIdQuery query, CancellationToken cancellationToken = default)
    {
        await _authorization.EnsurePermissionAsync(PermissionKeys.CharityRead, cancellationToken);
        var charityCase = await GetCaseOrThrowAsync(query.Id, cancellationToken);
        return charityCase.ToDto();
    }

    public async Task<PagedResult<CharityCaseListDto>> SearchAsync(SearchCharityCasesQuery query, CancellationToken cancellationToken = default)
    {
        await _authorization.EnsurePermissionAsync(PermissionKeys.CharityRead, cancellationToken);
        var result = await _charityCases.SearchAsync(query.PatientId, query.CaseNumber, query.Page, cancellationToken);
        return result.Map(CharityCaseMapper.ToListDto);
    }

    public Task<CharityCaseDto> HandleAsync(CreateCharityCaseCommand command, CancellationToken cancellationToken = default) => CreateAsync(command, cancellationToken);
    public Task<CharityCaseDto> HandleAsync(UpdateCharityCaseCommand command, CancellationToken cancellationToken = default) => UpdateAsync(command, cancellationToken);
    public Task<OperationResult> HandleAsync(DeleteCharityCaseCommand command, CancellationToken cancellationToken = default) => DeleteAsync(command, cancellationToken);
    public Task<CharityCaseDto> HandleAsync(GetCharityCaseByIdQuery query, CancellationToken cancellationToken = default) => GetByIdAsync(query, cancellationToken);
    public Task<PagedResult<CharityCaseListDto>> HandleAsync(SearchCharityCasesQuery query, CancellationToken cancellationToken = default) => SearchAsync(query, cancellationToken);

    private async Task<CharityCase> GetCaseOrThrowAsync(long id, CancellationToken cancellationToken)
    {
        return await _charityCases.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(CharityCase), id);
    }

    private static CharityCase CreateEntity(CreateCharityCaseCommand command)
    {
        return new CharityCase
        {
            PatientId = command.PatientId,
            CaseNumber = command.CaseNumber.Trim(),
            EligibilityStatus = command.EligibilityStatus,
            CoveragePercentage = command.CoveragePercentage,
            ValidFrom = command.ValidFrom,
            ValidTo = command.ValidTo,
            Notes = command.Notes?.Trim()
        };
    }

    private static void Apply(UpdateCharityCaseCommand command, CharityCase charityCase)
    {
        charityCase.EligibilityStatus = command.EligibilityStatus;
        charityCase.CoveragePercentage = command.CoveragePercentage;
        charityCase.ValidFrom = command.ValidFrom;
        charityCase.ValidTo = command.ValidTo;
        charityCase.Notes = command.Notes?.Trim();
        charityCase.UpdatedAt = DateTime.UtcNow;
    }
}

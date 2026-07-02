using AlAtaaClinic.Application.Abstractions.Logging;
using AlAtaaClinic.Application.Abstractions.Persistence;
using AlAtaaClinic.Application.Abstractions.Security;
using AlAtaaClinic.Application.Common.Exceptions;
using AlAtaaClinic.Application.Common.Messaging;
using AlAtaaClinic.Application.Common.Models;
using AlAtaaClinic.Application.Common.Validation;
using AlAtaaClinic.Domain.Core;

namespace AlAtaaClinic.Application.Features.Branches;

public sealed class BranchService :
    IBranchService,
    ICommandHandler<CreateBranchCommand, BranchDto>,
    ICommandHandler<UpdateBranchCommand, BranchDto>,
    ICommandHandler<DeleteBranchCommand, OperationResult>,
    IQueryHandler<GetBranchByIdQuery, BranchDto>,
    IQueryHandler<SearchBranchesQuery, PagedResult<BranchListDto>>
{
    private readonly IBranchRepository _branches;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthorizationService _authorization;
    private readonly ValidationRunner<CreateBranchCommand> _createValidator;
    private readonly ValidationRunner<UpdateBranchCommand> _updateValidator;
    private readonly IAppLogger<BranchService> _logger;

    public BranchService(
        IBranchRepository branches,
        IUnitOfWork unitOfWork,
        IAuthorizationService authorization,
        ValidationRunner<CreateBranchCommand> createValidator,
        ValidationRunner<UpdateBranchCommand> updateValidator,
        IAppLogger<BranchService> logger)
    {
        _branches = branches;
        _unitOfWork = unitOfWork;
        _authorization = authorization;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _logger = logger;
    }

    public async Task<BranchDto> CreateAsync(CreateBranchCommand command, CancellationToken cancellationToken = default)
    {
        await _authorization.EnsurePermissionAsync(PermissionKeys.StaffWrite, cancellationToken);
        _createValidator.ValidateAndThrow(command);
        await EnsureBranchCodeIsUniqueAsync(command.BranchCode, cancellationToken);

        var branch = CreateEntity(command);
        await _branches.AddAsync(branch, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.Information($"Branch '{branch.BranchCode}' created.");
        return branch.ToDto();
    }

    public async Task<BranchDto> UpdateAsync(UpdateBranchCommand command, CancellationToken cancellationToken = default)
    {
        await _authorization.EnsurePermissionAsync(PermissionKeys.StaffWrite, cancellationToken);
        _updateValidator.ValidateAndThrow(command);

        var branch = await GetBranchOrThrowAsync(command.Id, cancellationToken);
        Apply(command, branch);
        _branches.Update(branch);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return branch.ToDto();
    }

    public async Task<OperationResult> DeleteAsync(DeleteBranchCommand command, CancellationToken cancellationToken = default)
    {
        await _authorization.EnsurePermissionAsync(PermissionKeys.StaffWrite, cancellationToken);
        var branch = await GetBranchOrThrowAsync(command.Id, cancellationToken);
        branch.IsActive = false;
        _branches.Update(branch);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult.Success("Branch deactivated.");
    }

    public async Task<BranchDto> GetByIdAsync(GetBranchByIdQuery query, CancellationToken cancellationToken = default)
    {
        await _authorization.EnsurePermissionAsync(PermissionKeys.StaffRead, cancellationToken);
        var branch = await GetBranchOrThrowAsync(query.Id, cancellationToken);
        return branch.ToDto();
    }

    public async Task<PagedResult<BranchListDto>> SearchAsync(SearchBranchesQuery query, CancellationToken cancellationToken = default)
    {
        await _authorization.EnsurePermissionAsync(PermissionKeys.StaffRead, cancellationToken);
        var result = await _branches.SearchAsync(query.SearchText, query.Page, cancellationToken);
        return result.Map(BranchMapper.ToListDto);
    }

    public Task<BranchDto> HandleAsync(CreateBranchCommand command, CancellationToken cancellationToken = default)
    {
        return CreateAsync(command, cancellationToken);
    }

    public Task<BranchDto> HandleAsync(UpdateBranchCommand command, CancellationToken cancellationToken = default)
    {
        return UpdateAsync(command, cancellationToken);
    }

    public Task<OperationResult> HandleAsync(DeleteBranchCommand command, CancellationToken cancellationToken = default)
    {
        return DeleteAsync(command, cancellationToken);
    }

    public Task<BranchDto> HandleAsync(GetBranchByIdQuery query, CancellationToken cancellationToken = default)
    {
        return GetByIdAsync(query, cancellationToken);
    }

    public Task<PagedResult<BranchListDto>> HandleAsync(SearchBranchesQuery query, CancellationToken cancellationToken = default)
    {
        return SearchAsync(query, cancellationToken);
    }

    private async Task<Branch> GetBranchOrThrowAsync(long id, CancellationToken cancellationToken)
    {
        return await _branches.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Branch), id);
    }

    private async Task EnsureBranchCodeIsUniqueAsync(string branchCode, CancellationToken cancellationToken)
    {
        if (await _branches.GetByCodeAsync(branchCode, cancellationToken) is not null)
        {
            throw new ConflictException($"Branch code '{branchCode}' already exists.");
        }
    }

    private static Branch CreateEntity(CreateBranchCommand command)
    {
        return new Branch
        {
            BranchCode = command.BranchCode.Trim(),
            ArabicName = command.ArabicName.Trim(),
            EnglishName = command.EnglishName?.Trim(),
            PhoneNumber = command.PhoneNumber?.Trim(),
            AddressLine = command.AddressLine?.Trim()
        };
    }

    private static void Apply(UpdateBranchCommand command, Branch branch)
    {
        branch.ArabicName = command.ArabicName.Trim();
        branch.EnglishName = command.EnglishName?.Trim();
        branch.PhoneNumber = command.PhoneNumber?.Trim();
        branch.AddressLine = command.AddressLine?.Trim();
        branch.IsActive = command.IsActive;
        branch.UpdatedAt = DateTime.UtcNow;
    }
}

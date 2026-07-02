using AlAtaaClinic.Application.Abstractions.Logging;
using AlAtaaClinic.Application.Abstractions.Persistence;
using AlAtaaClinic.Application.Abstractions.Security;
using AlAtaaClinic.Application.Common.Exceptions;
using AlAtaaClinic.Application.Common.Messaging;
using AlAtaaClinic.Application.Common.Models;
using AlAtaaClinic.Application.Common.Validation;
using AlAtaaClinic.Domain.Core;


namespace AlAtaaClinic.Application.Features.StaffMembers;

public sealed class StaffMemberService :
    IStaffMemberService,
    ICommandHandler<CreateStaffMemberCommand, StaffMemberDto>,
    ICommandHandler<UpdateStaffMemberCommand, StaffMemberDto>,
    ICommandHandler<DeleteStaffMemberCommand, OperationResult>,
    IQueryHandler<GetStaffMemberByIdQuery, StaffMemberDto>,
    IQueryHandler<SearchStaffMembersQuery, PagedResult<StaffMemberListDto>>
{
    private readonly IStaffMemberRepository _staffMembers;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthorizationService _authorization;
    private readonly ValidationRunner<CreateStaffMemberCommand> _createValidator;
    private readonly ValidationRunner<UpdateStaffMemberCommand> _updateValidator;
    private readonly IAppLogger<StaffMemberService> _logger;

    public StaffMemberService(
        IStaffMemberRepository staffMembers,
        IUnitOfWork unitOfWork,
        IAuthorizationService authorization,
        ValidationRunner<CreateStaffMemberCommand> createValidator,
        ValidationRunner<UpdateStaffMemberCommand> updateValidator,
        IAppLogger<StaffMemberService> logger)
    {
        _staffMembers = staffMembers;
        _unitOfWork = unitOfWork;
        _authorization = authorization;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _logger = logger;
    }

    public async Task<StaffMemberDto> CreateAsync(CreateStaffMemberCommand command, CancellationToken cancellationToken = default)
    {
        await _authorization.EnsurePermissionAsync(PermissionKeys.StaffWrite, cancellationToken);
        _createValidator.ValidateAndThrow(command);
        await EnsureStaffCodeIsUniqueAsync(command.StaffCode, cancellationToken);

        var staffMember = CreateEntity(command);
        await _staffMembers.AddAsync(staffMember, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.Information($"StaffMember '{staffMember.StaffCode}' created.");
        return (await LoadWithReferencesAsync(staffMember.Id, cancellationToken))!.ToDto();
    }

    public async Task<StaffMemberDto> UpdateAsync(UpdateStaffMemberCommand command, CancellationToken cancellationToken = default)
    {
        await _authorization.EnsurePermissionAsync(PermissionKeys.StaffWrite, cancellationToken);
        _updateValidator.ValidateAndThrow(command);

        var staffMember = await GetStaffMemberOrThrowAsync(command.Id, cancellationToken);
        Apply(command, staffMember);
        _staffMembers.Update(staffMember);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return (await LoadWithReferencesAsync(staffMember.Id, cancellationToken))!.ToDto();
    }

    public async Task<OperationResult> DeleteAsync(DeleteStaffMemberCommand command, CancellationToken cancellationToken = default)
    {
        await _authorization.EnsurePermissionAsync(PermissionKeys.StaffWrite, cancellationToken);
        var staffMember = await GetStaffMemberOrThrowAsync(command.Id, cancellationToken);
        staffMember.IsActive = false;
        _staffMembers.Update(staffMember);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult.Success("Staff member deactivated.");
    }

    public async Task<StaffMemberDto> GetByIdAsync(GetStaffMemberByIdQuery query, CancellationToken cancellationToken = default)
    {
        await _authorization.EnsurePermissionAsync(PermissionKeys.StaffRead, cancellationToken);
        var staffMember = await LoadWithReferencesAsync(query.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(StaffMember), query.Id);
        return staffMember.ToDto();
    }

    public async Task<PagedResult<StaffMemberListDto>> SearchAsync(SearchStaffMembersQuery query, CancellationToken cancellationToken = default)
    {
        await _authorization.EnsurePermissionAsync(PermissionKeys.StaffRead, cancellationToken);
        var result = await _staffMembers.SearchAsync(query.SearchText, query.Page, cancellationToken);
        return result.Map(StaffMemberMapper.ToListDto);
    }

    public Task<StaffMemberDto> HandleAsync(CreateStaffMemberCommand command, CancellationToken cancellationToken = default)
    {
        return CreateAsync(command, cancellationToken);
    }

    public Task<StaffMemberDto> HandleAsync(UpdateStaffMemberCommand command, CancellationToken cancellationToken = default)
    {
        return UpdateAsync(command, cancellationToken);
    }

    public Task<OperationResult> HandleAsync(DeleteStaffMemberCommand command, CancellationToken cancellationToken = default)
    {
        return DeleteAsync(command, cancellationToken);
    }

    public Task<StaffMemberDto> HandleAsync(GetStaffMemberByIdQuery query, CancellationToken cancellationToken = default)
    {
        return GetByIdAsync(query, cancellationToken);
    }

    public Task<PagedResult<StaffMemberListDto>> HandleAsync(SearchStaffMembersQuery query, CancellationToken cancellationToken = default)
    {
        return SearchAsync(query, cancellationToken);
    }

    private async Task<StaffMember?> LoadWithReferencesAsync(long id, CancellationToken cancellationToken)
    {
        return await _staffMembers.GetWithDetailsAsync(id, cancellationToken);
    }

    private async Task<StaffMember> GetStaffMemberOrThrowAsync(long id, CancellationToken cancellationToken)
    {
        return await _staffMembers.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(StaffMember), id);
    }

    private async Task EnsureStaffCodeIsUniqueAsync(string staffCode, CancellationToken cancellationToken)
    {
        if (await _staffMembers.GetByCodeAsync(staffCode, cancellationToken) is not null)
        {
            throw new ConflictException($"Staff code '{staffCode}' already exists.");
        }
    }

    private static StaffMember CreateEntity(CreateStaffMemberCommand command)
    {
        return new StaffMember
        {
            BranchId = command.BranchId,
            DepartmentId = command.DepartmentId,
            StaffCode = command.StaffCode.Trim(),
            FullNameArabic = command.FullNameArabic.Trim(),
            FullNameEnglish = command.FullNameEnglish?.Trim(),
            PhoneNumber = command.PhoneNumber?.Trim(),
            JobTitle = command.JobTitle?.Trim(),
            IsActive = true
        };
    }

    private static void Apply(UpdateStaffMemberCommand command, StaffMember staffMember)
    {
        staffMember.BranchId = command.BranchId;
        staffMember.DepartmentId = command.DepartmentId;
        staffMember.FullNameArabic = command.FullNameArabic.Trim();
        staffMember.FullNameEnglish = command.FullNameEnglish?.Trim();
        staffMember.PhoneNumber = command.PhoneNumber?.Trim();
        staffMember.JobTitle = command.JobTitle?.Trim();
        staffMember.IsActive = command.IsActive;
        staffMember.UpdatedAt = DateTime.UtcNow;
    }
}

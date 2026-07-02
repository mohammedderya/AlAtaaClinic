using AlAtaaClinic.Application.Abstractions.Logging;
using AlAtaaClinic.Application.Abstractions.Persistence;
using AlAtaaClinic.Application.Abstractions.Security;
using AlAtaaClinic.Application.Common.Exceptions;
using AlAtaaClinic.Application.Common.Messaging;
using AlAtaaClinic.Application.Common.Models;
using AlAtaaClinic.Application.Common.Validation;
using AlAtaaClinic.Domain.Core;

namespace AlAtaaClinic.Application.Features.Departments;

public sealed class DepartmentService :
    IDepartmentService,
    ICommandHandler<CreateDepartmentCommand, DepartmentDto>,
    ICommandHandler<UpdateDepartmentCommand, DepartmentDto>,
    ICommandHandler<DeleteDepartmentCommand, OperationResult>,
    IQueryHandler<GetDepartmentByIdQuery, DepartmentDto>,
    IQueryHandler<SearchDepartmentsQuery, PagedResult<DepartmentListDto>>
{
    private readonly IDepartmentRepository _departments;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthorizationService _authorization;
    private readonly ValidationRunner<CreateDepartmentCommand> _createValidator;
    private readonly ValidationRunner<UpdateDepartmentCommand> _updateValidator;
    private readonly IAppLogger<DepartmentService> _logger;

    public DepartmentService(
        IDepartmentRepository departments,
        IUnitOfWork unitOfWork,
        IAuthorizationService authorization,
        ValidationRunner<CreateDepartmentCommand> createValidator,
        ValidationRunner<UpdateDepartmentCommand> updateValidator,
        IAppLogger<DepartmentService> logger)
    {
        _departments = departments;
        _unitOfWork = unitOfWork;
        _authorization = authorization;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _logger = logger;
    }

    public async Task<DepartmentDto> CreateAsync(CreateDepartmentCommand command, CancellationToken cancellationToken = default)
    {
        await _authorization.EnsurePermissionAsync(PermissionKeys.StaffWrite, cancellationToken);
        _createValidator.ValidateAndThrow(command);
        await EnsureNameIsUniqueAsync(command.BranchId, command.ArabicName, null, cancellationToken);

        var department = CreateEntity(command);
        await _departments.AddAsync(department, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.Information($"Department '{department.ArabicName}' created.");
        return department.ToDto();
    }

    public async Task<DepartmentDto> UpdateAsync(UpdateDepartmentCommand command, CancellationToken cancellationToken = default)
    {
        await _authorization.EnsurePermissionAsync(PermissionKeys.StaffWrite, cancellationToken);
        _updateValidator.ValidateAndThrow(command);

        var department = await GetDepartmentOrThrowAsync(command.Id, cancellationToken);
        await EnsureNameIsUniqueAsync(command.BranchId, command.ArabicName, command.Id, cancellationToken);

        Apply(command, department);
        _departments.Update(department);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return department.ToDto();
    }

    public async Task<OperationResult> DeleteAsync(DeleteDepartmentCommand command, CancellationToken cancellationToken = default)
    {
        await _authorization.EnsurePermissionAsync(PermissionKeys.StaffWrite, cancellationToken);
        var department = await GetDepartmentOrThrowAsync(command.Id, cancellationToken);
        department.IsActive = false;
        _departments.Update(department);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult.Success("Department deactivated.");
    }

    public async Task<DepartmentDto> GetByIdAsync(GetDepartmentByIdQuery query, CancellationToken cancellationToken = default)
    {
        await _authorization.EnsurePermissionAsync(PermissionKeys.StaffRead, cancellationToken);
        var department = await GetDepartmentOrThrowAsync(query.Id, cancellationToken);
        return department.ToDto();
    }

    public async Task<PagedResult<DepartmentListDto>> SearchAsync(SearchDepartmentsQuery query, CancellationToken cancellationToken = default)
    {
        await _authorization.EnsurePermissionAsync(PermissionKeys.StaffRead, cancellationToken);
        var result = await _departments.SearchAsync(query.SearchText, query.BranchId, query.Page, cancellationToken);
        return result.Map(DepartmentMapper.ToListDto);
    }

    public Task<DepartmentDto> HandleAsync(CreateDepartmentCommand command, CancellationToken cancellationToken = default)
    {
        return CreateAsync(command, cancellationToken);
    }

    public Task<DepartmentDto> HandleAsync(UpdateDepartmentCommand command, CancellationToken cancellationToken = default)
    {
        return UpdateAsync(command, cancellationToken);
    }

    public Task<OperationResult> HandleAsync(DeleteDepartmentCommand command, CancellationToken cancellationToken = default)
    {
        return DeleteAsync(command, cancellationToken);
    }

    public Task<DepartmentDto> HandleAsync(GetDepartmentByIdQuery query, CancellationToken cancellationToken = default)
    {
        return GetByIdAsync(query, cancellationToken);
    }

    public Task<PagedResult<DepartmentListDto>> HandleAsync(SearchDepartmentsQuery query, CancellationToken cancellationToken = default)
    {
        return SearchAsync(query, cancellationToken);
    }

    private async Task<Department> GetDepartmentOrThrowAsync(long id, CancellationToken cancellationToken)
    {
        return await _departments.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Department), id);
    }

    private async Task EnsureNameIsUniqueAsync(long branchId, string arabicName, long? excludeId, CancellationToken cancellationToken)
    {
        if (!await _departments.IsNameUniqueAsync(branchId, arabicName, excludeId, cancellationToken))
        {
            throw new ConflictException($"Department '{arabicName}' already exists in this branch.");
        }
    }

    private static Department CreateEntity(CreateDepartmentCommand command)
    {
        return new Department
        {
            BranchId = command.BranchId,
            ArabicName = command.ArabicName.Trim(),
            EnglishName = command.EnglishName?.Trim()
        };
    }

    private static void Apply(UpdateDepartmentCommand command, Department department)
    {
        department.BranchId = command.BranchId;
        department.ArabicName = command.ArabicName.Trim();
        department.EnglishName = command.EnglishName?.Trim();
        department.IsActive = command.IsActive;
        department.UpdatedAt = DateTime.UtcNow;
    }
}

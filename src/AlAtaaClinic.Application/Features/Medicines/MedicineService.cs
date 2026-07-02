using AlAtaaClinic.Application.Abstractions.Logging;
using AlAtaaClinic.Application.Abstractions.Persistence;
using AlAtaaClinic.Application.Abstractions.Security;
using AlAtaaClinic.Application.Common.Exceptions;
using AlAtaaClinic.Application.Common.Messaging;
using AlAtaaClinic.Application.Common.Models;
using AlAtaaClinic.Application.Common.Validation;
using AlAtaaClinic.Domain.Inventory;

namespace AlAtaaClinic.Application.Features.Medicines;

public sealed class MedicineService :
    IMedicineService,
    ICommandHandler<CreateMedicineCommand, MedicineDto>,
    ICommandHandler<UpdateMedicineCommand, MedicineDto>,
    ICommandHandler<DeleteMedicineCommand, OperationResult>,
    IQueryHandler<GetMedicineByIdQuery, MedicineDto>,
    IQueryHandler<SearchMedicinesQuery, PagedResult<MedicineListDto>>
{
    private readonly IMedicineRepository _medicines;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthorizationService _authorization;
    private readonly ValidationRunner<CreateMedicineCommand> _createValidator;
    private readonly ValidationRunner<UpdateMedicineCommand> _updateValidator;
    private readonly IAppLogger<MedicineService> _logger;

    public MedicineService(
        IMedicineRepository medicines,
        IUnitOfWork unitOfWork,
        IAuthorizationService authorization,
        ValidationRunner<CreateMedicineCommand> createValidator,
        ValidationRunner<UpdateMedicineCommand> updateValidator,
        IAppLogger<MedicineService> logger)
    {
        _medicines = medicines;
        _unitOfWork = unitOfWork;
        _authorization = authorization;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _logger = logger;
    }

    public async Task<MedicineDto> CreateAsync(CreateMedicineCommand command, CancellationToken cancellationToken = default)
    {
        await _authorization.EnsurePermissionAsync(PermissionKeys.InventoryWrite, cancellationToken);
        _createValidator.ValidateAndThrow(command);
        await EnsureCodeIsUniqueAsync(command.MedicineCode, cancellationToken);

        var medicine = CreateEntity(command);
        await _medicines.AddAsync(medicine, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.Information($"Medicine '{medicine.MedicineCode}' created.");
        return medicine.ToDto();
    }

    public async Task<MedicineDto> UpdateAsync(UpdateMedicineCommand command, CancellationToken cancellationToken = default)
    {
        await _authorization.EnsurePermissionAsync(PermissionKeys.InventoryWrite, cancellationToken);
        _updateValidator.ValidateAndThrow(command);

        var medicine = await GetMedicineOrThrowAsync(command.Id, cancellationToken);
        Apply(command, medicine);
        _medicines.Update(medicine);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return medicine.ToDto();
    }

    public async Task<OperationResult> DeleteAsync(DeleteMedicineCommand command, CancellationToken cancellationToken = default)
    {
        await _authorization.EnsurePermissionAsync(PermissionKeys.InventoryWrite, cancellationToken);
        var medicine = await GetMedicineOrThrowAsync(command.Id, cancellationToken);
        medicine.IsActive = false;
        medicine.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult.Success("Medicine deactivated.");
    }

    public async Task<MedicineDto> GetByIdAsync(GetMedicineByIdQuery query, CancellationToken cancellationToken = default)
    {
        await _authorization.EnsurePermissionAsync(PermissionKeys.InventoryRead, cancellationToken);
        var medicine = await GetMedicineOrThrowAsync(query.Id, cancellationToken);
        return medicine.ToDto();
    }

    public async Task<PagedResult<MedicineListDto>> SearchAsync(SearchMedicinesQuery query, CancellationToken cancellationToken = default)
    {
        await _authorization.EnsurePermissionAsync(PermissionKeys.InventoryRead, cancellationToken);
        var result = await _medicines.SearchAsync(query.SearchText, query.Page, cancellationToken);
        return result.Map(MedicineMapper.ToListDto);
    }

    public Task<MedicineDto> HandleAsync(CreateMedicineCommand command, CancellationToken cancellationToken = default) => CreateAsync(command, cancellationToken);
    public Task<MedicineDto> HandleAsync(UpdateMedicineCommand command, CancellationToken cancellationToken = default) => UpdateAsync(command, cancellationToken);
    public Task<OperationResult> HandleAsync(DeleteMedicineCommand command, CancellationToken cancellationToken = default) => DeleteAsync(command, cancellationToken);
    public Task<MedicineDto> HandleAsync(GetMedicineByIdQuery query, CancellationToken cancellationToken = default) => GetByIdAsync(query, cancellationToken);
    public Task<PagedResult<MedicineListDto>> HandleAsync(SearchMedicinesQuery query, CancellationToken cancellationToken = default) => SearchAsync(query, cancellationToken);

    private async Task<Medicine> GetMedicineOrThrowAsync(long id, CancellationToken cancellationToken)
    {
        return await _medicines.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Medicine), id);
    }

    private async Task EnsureCodeIsUniqueAsync(string medicineCode, CancellationToken cancellationToken)
    {
        if (await _medicines.GetByCodeAsync(medicineCode, cancellationToken) is not null)
        {
            throw new ConflictException($"Medicine code '{medicineCode}' already exists.");
        }
    }

    private static Medicine CreateEntity(CreateMedicineCommand command)
    {
        return new Medicine
        {
            MedicineCode = command.MedicineCode.Trim(),
            GenericName = command.GenericName.Trim(),
            TradeName = command.TradeName?.Trim(),
            Unit = command.Unit.Trim()
        };
    }

    private static void Apply(UpdateMedicineCommand command, Medicine medicine)
    {
        medicine.GenericName = command.GenericName.Trim();
        medicine.TradeName = command.TradeName?.Trim();
        medicine.Unit = command.Unit.Trim();
        medicine.IsActive = command.IsActive;
        medicine.UpdatedAt = DateTime.UtcNow;
    }
}

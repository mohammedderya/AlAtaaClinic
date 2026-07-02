using AlAtaaClinic.Application.Abstractions.Logging;
using AlAtaaClinic.Application.Abstractions.Persistence;
using AlAtaaClinic.Application.Abstractions.Security;
using AlAtaaClinic.Application.Common.Exceptions;
using AlAtaaClinic.Application.Common.Messaging;
using AlAtaaClinic.Application.Common.Models;
using AlAtaaClinic.Application.Common.Validation;
using AlAtaaClinic.Domain.Core;

namespace AlAtaaClinic.Application.Features.Doctors;

public sealed class DoctorService :
    IDoctorService,
    ICommandHandler<CreateDoctorCommand, DoctorDto>,
    ICommandHandler<UpdateDoctorCommand, DoctorDto>,
    ICommandHandler<DeleteDoctorCommand, OperationResult>,
    IQueryHandler<GetDoctorByIdQuery, DoctorDto>,
    IQueryHandler<SearchDoctorsQuery, PagedResult<DoctorListDto>>
{
    private readonly IDoctorRepository _doctors;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthorizationService _authorization;
    private readonly ValidationRunner<CreateDoctorCommand> _createValidator;
    private readonly ValidationRunner<UpdateDoctorCommand> _updateValidator;
    private readonly IAppLogger<DoctorService> _logger;

    public DoctorService(
        IDoctorRepository doctors,
        IUnitOfWork unitOfWork,
        IAuthorizationService authorization,
        ValidationRunner<CreateDoctorCommand> createValidator,
        ValidationRunner<UpdateDoctorCommand> updateValidator,
        IAppLogger<DoctorService> logger)
    {
        _doctors = doctors;
        _unitOfWork = unitOfWork;
        _authorization = authorization;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _logger = logger;
    }

    public async Task<DoctorDto> CreateAsync(CreateDoctorCommand command, CancellationToken cancellationToken = default)
    {
        await _authorization.EnsurePermissionAsync(PermissionKeys.StaffWrite, cancellationToken);
        _createValidator.ValidateAndThrow(command);
        await EnsureStaffMemberNotAlreadyDoctorAsync(command.StaffMemberId, cancellationToken);

        var doctor = CreateEntity(command);
        await _doctors.AddAsync(doctor, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.Information($"Doctor created for StaffMember #{command.StaffMemberId}.");
        return (await LoadWithStaffAsync(doctor.Id, cancellationToken))!.ToDto();
    }

    public async Task<DoctorDto> UpdateAsync(UpdateDoctorCommand command, CancellationToken cancellationToken = default)
    {
        await _authorization.EnsurePermissionAsync(PermissionKeys.StaffWrite, cancellationToken);
        _updateValidator.ValidateAndThrow(command);

        var doctor = await GetDoctorOrThrowAsync(command.Id, cancellationToken);
        Apply(command, doctor);
        _doctors.Update(doctor);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return (await LoadWithStaffAsync(doctor.Id, cancellationToken))!.ToDto();
    }

    public async Task<OperationResult> DeleteAsync(DeleteDoctorCommand command, CancellationToken cancellationToken = default)
    {
        await _authorization.EnsurePermissionAsync(PermissionKeys.StaffWrite, cancellationToken);
        var doctor = await GetDoctorOrThrowAsync(command.Id, cancellationToken);
        doctor.IsActive = false;
        _doctors.Update(doctor);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult.Success("Doctor deactivated.");
    }

    public async Task<DoctorDto> GetByIdAsync(GetDoctorByIdQuery query, CancellationToken cancellationToken = default)
    {
        await _authorization.EnsurePermissionAsync(PermissionKeys.StaffRead, cancellationToken);
        var doctor = await LoadWithStaffAsync(query.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Doctor), query.Id);
        return doctor.ToDto();
    }

    public async Task<PagedResult<DoctorListDto>> SearchAsync(SearchDoctorsQuery query, CancellationToken cancellationToken = default)
    {
        await _authorization.EnsurePermissionAsync(PermissionKeys.StaffRead, cancellationToken);
        var result = await _doctors.SearchAsync(query.SearchText, query.Page, cancellationToken);
        return result.Map(DoctorMapper.ToListDto);
    }

    public Task<DoctorDto> HandleAsync(CreateDoctorCommand command, CancellationToken cancellationToken = default)
    {
        return CreateAsync(command, cancellationToken);
    }

    public Task<DoctorDto> HandleAsync(UpdateDoctorCommand command, CancellationToken cancellationToken = default)
    {
        return UpdateAsync(command, cancellationToken);
    }

    public Task<OperationResult> HandleAsync(DeleteDoctorCommand command, CancellationToken cancellationToken = default)
    {
        return DeleteAsync(command, cancellationToken);
    }

    public Task<DoctorDto> HandleAsync(GetDoctorByIdQuery query, CancellationToken cancellationToken = default)
    {
        return GetByIdAsync(query, cancellationToken);
    }

    public Task<PagedResult<DoctorListDto>> HandleAsync(SearchDoctorsQuery query, CancellationToken cancellationToken = default)
    {
        return SearchAsync(query, cancellationToken);
    }

    private Task<Doctor?> LoadWithStaffAsync(long id, CancellationToken cancellationToken)
    {
        return _doctors.GetWithStaffAsync(id, cancellationToken);
    }

    private async Task<Doctor> GetDoctorOrThrowAsync(long id, CancellationToken cancellationToken)
    {
        return await _doctors.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Doctor), id);
    }

    private async Task EnsureStaffMemberNotAlreadyDoctorAsync(long staffMemberId, CancellationToken cancellationToken)
    {
        if (await _doctors.GetByStaffMemberIdAsync(staffMemberId, cancellationToken) is not null)
        {
            throw new ConflictException("This staff member is already registered as a doctor.");
        }
    }

    private static Doctor CreateEntity(CreateDoctorCommand command)
    {
        return new Doctor
        {
            StaffMemberId = command.StaffMemberId,
            Specialty = command.Specialty.Trim(),
            LicenseNumber = command.LicenseNumber?.Trim(),
            IsActive = true
        };
    }

    private static void Apply(UpdateDoctorCommand command, Doctor doctor)
    {
        doctor.Specialty = command.Specialty.Trim();
        doctor.LicenseNumber = command.LicenseNumber?.Trim();
        doctor.IsActive = command.IsActive;
    }
}

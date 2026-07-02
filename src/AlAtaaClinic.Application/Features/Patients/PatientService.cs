using AlAtaaClinic.Application.Abstractions.Audit;
using AlAtaaClinic.Application.Abstractions.Logging;
using AlAtaaClinic.Application.Abstractions.Persistence;
using AlAtaaClinic.Application.Abstractions.Security;
using AlAtaaClinic.Application.Common.Exceptions;
using AlAtaaClinic.Application.Common.Messaging;
using AlAtaaClinic.Application.Common.Models;
using AlAtaaClinic.Application.Common.Validation;
using AlAtaaClinic.Domain.Core;
using AlAtaaClinic.Domain.Enums;

namespace AlAtaaClinic.Application.Features.Patients;

public sealed class PatientService :
    IPatientService,
    ICommandHandler<CreatePatientCommand, PatientDto>,
    ICommandHandler<UpdatePatientCommand, PatientDto>,
    ICommandHandler<DeletePatientCommand, OperationResult>,
    IQueryHandler<GetPatientByIdQuery, PatientDto>,
    IQueryHandler<SearchPatientsQuery, PagedResult<PatientListDto>>
{
    private readonly IPatientRepository _patients;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthorizationService _authorization;
    private readonly ValidationRunner<CreatePatientCommand> _createValidator;
    private readonly ValidationRunner<UpdatePatientCommand> _updateValidator;
    private readonly IAuditService _audit;
    private readonly IAppLogger<PatientService> _logger;

    public PatientService(
        IPatientRepository patients,
        IUnitOfWork unitOfWork,
        IAuthorizationService authorization,
        ValidationRunner<CreatePatientCommand> createValidator,
        ValidationRunner<UpdatePatientCommand> updateValidator,
        IAuditService audit,
        IAppLogger<PatientService> logger)
    {
        _patients = patients;
        _unitOfWork = unitOfWork;
        _authorization = authorization;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _audit = audit;
        _logger = logger;
    }

    public async Task<PatientDto> CreateAsync(CreatePatientCommand command, CancellationToken cancellationToken = default)
    {
        await _authorization.EnsurePermissionAsync(PermissionKeys.PatientsWrite, cancellationToken);
        _createValidator.ValidateAndThrow(command);
        await EnsureFileNumberIsUniqueAsync(command.FileNumber, cancellationToken);

        var patient = CreateEntity(command);
        await _patients.AddAsync(patient, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync(nameof(Patient), patient.Id.ToString(), AuditAction.Create, newValues: patient.FileNumber, cancellationToken: cancellationToken);
        _logger.Information($"Patient '{patient.FileNumber}' created.");
        return patient.ToDto();
    }

    public async Task<PatientDto> UpdateAsync(UpdatePatientCommand command, CancellationToken cancellationToken = default)
    {
        await _authorization.EnsurePermissionAsync(PermissionKeys.PatientsWrite, cancellationToken);
        _updateValidator.ValidateAndThrow(command);

        var patient = await GetPatientOrThrowAsync(command.Id, cancellationToken);
        Apply(command, patient);
        _patients.Update(patient);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync(nameof(Patient), patient.Id.ToString(), AuditAction.Update, newValues: patient.FileNumber, cancellationToken: cancellationToken);
        return patient.ToDto();
    }

    public async Task<OperationResult> DeleteAsync(DeletePatientCommand command, CancellationToken cancellationToken = default)
    {
        await _authorization.EnsurePermissionAsync(PermissionKeys.PatientsWrite, cancellationToken);
        var patient = await GetPatientOrThrowAsync(command.Id, cancellationToken);
        patient.IsActive = false;
        _patients.Update(patient);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync(nameof(Patient), patient.Id.ToString(), AuditAction.Delete, cancellationToken: cancellationToken);
        return OperationResult.Success("Patient deactivated.");
    }

    public async Task<PatientDto> GetByIdAsync(GetPatientByIdQuery query, CancellationToken cancellationToken = default)
    {
        await _authorization.EnsurePermissionAsync(PermissionKeys.PatientsRead, cancellationToken);
        var patient = await GetPatientOrThrowAsync(query.Id, cancellationToken);
        return patient.ToDto();
    }

    public async Task<PagedResult<PatientListDto>> SearchAsync(SearchPatientsQuery query, CancellationToken cancellationToken = default)
    {
        await _authorization.EnsurePermissionAsync(PermissionKeys.PatientsRead, cancellationToken);
        var result = await _patients.SearchAsync(query.SearchText, query.Page, cancellationToken);
        return result.Map(PatientMapper.ToListDto);
    }

    public Task<PatientDto> HandleAsync(CreatePatientCommand command, CancellationToken cancellationToken = default)
    {
        return CreateAsync(command, cancellationToken);
    }

    public Task<PatientDto> HandleAsync(UpdatePatientCommand command, CancellationToken cancellationToken = default)
    {
        return UpdateAsync(command, cancellationToken);
    }

    public Task<OperationResult> HandleAsync(DeletePatientCommand command, CancellationToken cancellationToken = default)
    {
        return DeleteAsync(command, cancellationToken);
    }

    public Task<PatientDto> HandleAsync(GetPatientByIdQuery query, CancellationToken cancellationToken = default)
    {
        return GetByIdAsync(query, cancellationToken);
    }

    public Task<PagedResult<PatientListDto>> HandleAsync(SearchPatientsQuery query, CancellationToken cancellationToken = default)
    {
        return SearchAsync(query, cancellationToken);
    }

    private async Task<Patient> GetPatientOrThrowAsync(long id, CancellationToken cancellationToken)
    {
        return await _patients.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Patient), id);
    }

    private async Task EnsureFileNumberIsUniqueAsync(string fileNumber, CancellationToken cancellationToken)
    {
        if (await _patients.GetByFileNumberAsync(fileNumber, cancellationToken) is not null)
        {
            throw new ConflictException($"Patient file number '{fileNumber}' already exists.");
        }
    }

    private static Patient CreateEntity(CreatePatientCommand command)
    {
        return new Patient
        {
            FileNumber = command.FileNumber.Trim(),
            NationalId = command.NationalId?.Trim(),
            FullNameArabic = command.FullNameArabic.Trim(),
            FullNameEnglish = command.FullNameEnglish?.Trim(),
            Gender = command.Gender,
            DateOfBirth = command.DateOfBirth,
            PhoneNumber = command.PhoneNumber?.Trim(),
            AlternativePhoneNumber = command.AlternativePhoneNumber?.Trim(),
            AddressLine = command.AddressLine?.Trim(),
            BloodType = command.BloodType?.Trim()
        };
    }

    private static void Apply(UpdatePatientCommand command, Patient patient)
    {
        patient.NationalId = command.NationalId?.Trim();
        patient.FullNameArabic = command.FullNameArabic.Trim();
        patient.FullNameEnglish = command.FullNameEnglish?.Trim();
        patient.Gender = command.Gender;
        patient.DateOfBirth = command.DateOfBirth;
        patient.PhoneNumber = command.PhoneNumber?.Trim();
        patient.AlternativePhoneNumber = command.AlternativePhoneNumber?.Trim();
        patient.AddressLine = command.AddressLine?.Trim();
        patient.BloodType = command.BloodType?.Trim();
        patient.IsActive = command.IsActive;
        patient.UpdatedAt = DateTime.UtcNow;
    }
}

using AlAtaaClinic.Application.Abstractions.Logging;
using AlAtaaClinic.Application.Abstractions.Persistence;
using AlAtaaClinic.Application.Abstractions.Security;
using AlAtaaClinic.Application.Common.Exceptions;
using AlAtaaClinic.Application.Common.Messaging;
using AlAtaaClinic.Application.Common.Models;
using AlAtaaClinic.Application.Common.Validation;
using AlAtaaClinic.Domain.Clinical;

namespace AlAtaaClinic.Application.Features.Appointments;

public sealed class AppointmentService :
    IAppointmentService,
    ICommandHandler<CreateAppointmentCommand, AppointmentDto>,
    ICommandHandler<UpdateAppointmentCommand, AppointmentDto>,
    ICommandHandler<DeleteAppointmentCommand, OperationResult>,
    IQueryHandler<GetAppointmentByIdQuery, AppointmentDto>,
    IQueryHandler<SearchAppointmentsQuery, PagedResult<AppointmentListDto>>
{
    private readonly IAppointmentRepository _appointments;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthorizationService _authorization;
    private readonly ValidationRunner<CreateAppointmentCommand> _createValidator;
    private readonly ValidationRunner<UpdateAppointmentCommand> _updateValidator;
    private readonly IAppLogger<AppointmentService> _logger;

    public AppointmentService(
        IAppointmentRepository appointments,
        IUnitOfWork unitOfWork,
        IAuthorizationService authorization,
        ValidationRunner<CreateAppointmentCommand> createValidator,
        ValidationRunner<UpdateAppointmentCommand> updateValidator,
        IAppLogger<AppointmentService> logger)
    {
        _appointments = appointments;
        _unitOfWork = unitOfWork;
        _authorization = authorization;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _logger = logger;
    }

    public async Task<AppointmentDto> CreateAsync(CreateAppointmentCommand command, CancellationToken cancellationToken = default)
    {
        await _authorization.EnsurePermissionAsync(PermissionKeys.SchedulingWrite, cancellationToken);
        _createValidator.ValidateAndThrow(command);

        var appointment = CreateEntity(command);
        await _appointments.AddAsync(appointment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.Information($"Appointment '{appointment.Id}' created.");
        return appointment.ToDto();
    }

    public async Task<AppointmentDto> UpdateAsync(UpdateAppointmentCommand command, CancellationToken cancellationToken = default)
    {
        await _authorization.EnsurePermissionAsync(PermissionKeys.SchedulingWrite, cancellationToken);
        _updateValidator.ValidateAndThrow(command);

        var appointment = await GetAppointmentOrThrowAsync(command.Id, cancellationToken);
        Apply(command, appointment);
        _appointments.Update(appointment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return appointment.ToDto();
    }

    public async Task<OperationResult> DeleteAsync(DeleteAppointmentCommand command, CancellationToken cancellationToken = default)
    {
        await _authorization.EnsurePermissionAsync(PermissionKeys.SchedulingWrite, cancellationToken);
        var appointment = await GetAppointmentOrThrowAsync(command.Id, cancellationToken);
        _appointments.Remove(appointment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult.Success("Appointment deleted.");
    }

    public async Task<AppointmentDto> GetByIdAsync(GetAppointmentByIdQuery query, CancellationToken cancellationToken = default)
    {
        await _authorization.EnsurePermissionAsync(PermissionKeys.SchedulingRead, cancellationToken);
        var appointment = await GetAppointmentOrThrowAsync(query.Id, cancellationToken);
        return appointment.ToDto();
    }

    public async Task<PagedResult<AppointmentListDto>> SearchAsync(SearchAppointmentsQuery query, CancellationToken cancellationToken = default)
    {
        await _authorization.EnsurePermissionAsync(PermissionKeys.SchedulingRead, cancellationToken);
        var result = await _appointments.SearchAsync(query.DoctorId, query.From, query.To, query.Page, cancellationToken);
        return result.Map(AppointmentMapper.ToListDto);
    }

    public Task<AppointmentDto> HandleAsync(CreateAppointmentCommand command, CancellationToken cancellationToken = default) => CreateAsync(command, cancellationToken);
    public Task<AppointmentDto> HandleAsync(UpdateAppointmentCommand command, CancellationToken cancellationToken = default) => UpdateAsync(command, cancellationToken);
    public Task<OperationResult> HandleAsync(DeleteAppointmentCommand command, CancellationToken cancellationToken = default) => DeleteAsync(command, cancellationToken);
    public Task<AppointmentDto> HandleAsync(GetAppointmentByIdQuery query, CancellationToken cancellationToken = default) => GetByIdAsync(query, cancellationToken);
    public Task<PagedResult<AppointmentListDto>> HandleAsync(SearchAppointmentsQuery query, CancellationToken cancellationToken = default) => SearchAsync(query, cancellationToken);

    private async Task<Appointment> GetAppointmentOrThrowAsync(long id, CancellationToken cancellationToken)
    {
        return await _appointments.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Appointment), id);
    }

    private static Appointment CreateEntity(CreateAppointmentCommand command)
    {
        return new Appointment
        {
            BranchId = command.BranchId,
            PatientId = command.PatientId,
            DoctorId = command.DoctorId,
            AppointmentStart = command.AppointmentStart,
            AppointmentEnd = command.AppointmentEnd,
            Notes = command.Notes?.Trim()
        };
    }

    private static void Apply(UpdateAppointmentCommand command, Appointment appointment)
    {
        appointment.BranchId = command.BranchId;
        appointment.PatientId = command.PatientId;
        appointment.DoctorId = command.DoctorId;
        appointment.AppointmentStart = command.AppointmentStart;
        appointment.AppointmentEnd = command.AppointmentEnd;
        appointment.Status = command.Status;
        appointment.Notes = command.Notes?.Trim();
        appointment.UpdatedAt = DateTime.UtcNow;
    }
}

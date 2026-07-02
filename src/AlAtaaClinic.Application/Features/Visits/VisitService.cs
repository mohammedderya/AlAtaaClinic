using AlAtaaClinic.Application.Abstractions.Logging;
using AlAtaaClinic.Application.Abstractions.Persistence;
using AlAtaaClinic.Application.Abstractions.Security;
using AlAtaaClinic.Application.Common.Exceptions;
using AlAtaaClinic.Application.Common.Messaging;
using AlAtaaClinic.Application.Common.Models;
using AlAtaaClinic.Application.Common.Validation;
using AlAtaaClinic.Domain.Clinical;
using AlAtaaClinic.Domain.Enums;

namespace AlAtaaClinic.Application.Features.Visits;

public sealed class VisitService :
    IVisitService,
    ICommandHandler<CreateVisitCommand, VisitDto>,
    ICommandHandler<UpdateVisitCommand, VisitDto>,
    ICommandHandler<DeleteVisitCommand, OperationResult>,
    IQueryHandler<GetVisitByIdQuery, VisitDto>,
    IQueryHandler<SearchVisitsQuery, PagedResult<VisitListDto>>
{
    private readonly IVisitRepository _visits;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthorizationService _authorization;
    private readonly ValidationRunner<CreateVisitCommand> _createValidator;
    private readonly ValidationRunner<UpdateVisitCommand> _updateValidator;
    private readonly IAppLogger<VisitService> _logger;

    public VisitService(
        IVisitRepository visits,
        IUnitOfWork unitOfWork,
        IAuthorizationService authorization,
        ValidationRunner<CreateVisitCommand> createValidator,
        ValidationRunner<UpdateVisitCommand> updateValidator,
        IAppLogger<VisitService> logger)
    {
        _visits = visits;
        _unitOfWork = unitOfWork;
        _authorization = authorization;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _logger = logger;
    }

    public async Task<VisitDto> CreateAsync(CreateVisitCommand command, CancellationToken cancellationToken = default)
    {
        await _authorization.EnsurePermissionAsync(PermissionKeys.ClinicalWrite, cancellationToken);
        _createValidator.ValidateAndThrow(command);

        var visit = CreateEntity(command);
        await _visits.AddAsync(visit, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.Information($"Visit '{visit.Id}' created.");
        return visit.ToDto();
    }

    public async Task<VisitDto> UpdateAsync(UpdateVisitCommand command, CancellationToken cancellationToken = default)
    {
        await _authorization.EnsurePermissionAsync(PermissionKeys.ClinicalWrite, cancellationToken);
        _updateValidator.ValidateAndThrow(command);

        var visit = await GetVisitOrThrowAsync(command.Id, cancellationToken);
        Apply(command, visit);
        _visits.Update(visit);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return visit.ToDto();
    }

    public async Task<OperationResult> DeleteAsync(DeleteVisitCommand command, CancellationToken cancellationToken = default)
    {
        await _authorization.EnsurePermissionAsync(PermissionKeys.ClinicalWrite, cancellationToken);
        var visit = await GetVisitOrThrowAsync(command.Id, cancellationToken);
        visit.Status = VisitStatus.Cancelled;
        visit.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult.Success("Visit cancelled.");
    }

    public async Task<VisitDto> GetByIdAsync(GetVisitByIdQuery query, CancellationToken cancellationToken = default)
    {
        await _authorization.EnsurePermissionAsync(PermissionKeys.ClinicalRead, cancellationToken);
        var visit = await GetVisitOrThrowAsync(query.Id, cancellationToken);
        return visit.ToDto();
    }

    public async Task<PagedResult<VisitListDto>> SearchAsync(SearchVisitsQuery query, CancellationToken cancellationToken = default)
    {
        await _authorization.EnsurePermissionAsync(PermissionKeys.ClinicalRead, cancellationToken);
        var result = await _visits.SearchAsync(query.PatientId, query.DoctorId, query.From, query.To, query.Page, cancellationToken);
        return result.Map(VisitMapper.ToListDto);
    }

    public Task<VisitDto> HandleAsync(CreateVisitCommand command, CancellationToken cancellationToken = default) => CreateAsync(command, cancellationToken);
    public Task<VisitDto> HandleAsync(UpdateVisitCommand command, CancellationToken cancellationToken = default) => UpdateAsync(command, cancellationToken);
    public Task<OperationResult> HandleAsync(DeleteVisitCommand command, CancellationToken cancellationToken = default) => DeleteAsync(command, cancellationToken);
    public Task<VisitDto> HandleAsync(GetVisitByIdQuery query, CancellationToken cancellationToken = default) => GetByIdAsync(query, cancellationToken);
    public Task<PagedResult<VisitListDto>> HandleAsync(SearchVisitsQuery query, CancellationToken cancellationToken = default) => SearchAsync(query, cancellationToken);

    private async Task<Visit> GetVisitOrThrowAsync(long id, CancellationToken cancellationToken)
    {
        return await _visits.GetWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Visit), id);
    }

    private static Visit CreateEntity(CreateVisitCommand command)
    {
        return new Visit
        {
            BranchId = command.BranchId,
            PatientId = command.PatientId,
            DoctorId = command.DoctorId,
            AppointmentId = command.AppointmentId,
            ChiefComplaint = command.ChiefComplaint?.Trim(),
            ClinicalNotes = command.ClinicalNotes?.Trim(),
            VitalSigns = command.VitalSigns.Select(VisitMapper.ToEntity).ToList(),
            Diagnoses = command.Diagnoses.Select(VisitMapper.ToEntity).ToList(),
            Procedures = command.Procedures.Select(VisitMapper.ToEntity).ToList(),
            Prescriptions = command.Prescriptions.Select(VisitMapper.ToEntity).ToList(),
            MedicalOrders = command.MedicalOrders.Select(VisitMapper.ToEntity).ToList()
        };
    }

    private static void Apply(UpdateVisitCommand command, Visit visit)
    {
        visit.Status = command.Status;
        visit.ChiefComplaint = command.ChiefComplaint?.Trim();
        visit.ClinicalNotes = command.ClinicalNotes?.Trim();
        visit.ClosedAt = command.ClosedAt;
        visit.UpdatedAt = DateTime.UtcNow;
        ReplaceDetails(command, visit);
    }

    private static void ReplaceDetails(UpdateVisitCommand command, Visit visit)
    {
        ReplaceCollection(visit.VitalSigns, command.VitalSigns.Select(VisitMapper.ToEntity));
        ReplaceCollection(visit.Diagnoses, command.Diagnoses.Select(VisitMapper.ToEntity));
        ReplaceCollection(visit.Procedures, command.Procedures.Select(VisitMapper.ToEntity));
        ReplaceCollection(visit.Prescriptions, command.Prescriptions.Select(VisitMapper.ToEntity));
        ReplaceCollection(visit.MedicalOrders, command.MedicalOrders.Select(VisitMapper.ToEntity));
    }

    private static void ReplaceCollection<T>(ICollection<T> target, IEnumerable<T> source)
    {
        target.Clear();
        foreach (var item in source)
        {
            target.Add(item);
        }
    }
}

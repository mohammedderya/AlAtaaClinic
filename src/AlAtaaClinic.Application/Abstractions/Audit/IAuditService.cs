using AlAtaaClinic.Domain.Enums;

namespace AlAtaaClinic.Application.Abstractions.Audit;

public interface IAuditService
{
    Task LogAsync(
        string entityName,
        string? entityId,
        AuditAction action,
        string? oldValues = null,
        string? newValues = null,
        CancellationToken cancellationToken = default);
}

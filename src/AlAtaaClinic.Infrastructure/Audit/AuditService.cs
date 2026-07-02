using AlAtaaClinic.Application.Abstractions.Audit;
using AlAtaaClinic.Application.Abstractions.Security;
using AlAtaaClinic.Domain.Audit;
using AlAtaaClinic.Domain.Enums;
using AlAtaaClinic.Infrastructure.Persistence;

namespace AlAtaaClinic.Infrastructure.Audit;

public sealed class AuditService : IAuditService
{
    private readonly ClinicDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public AuditService(ClinicDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task LogAsync(
        string entityName,
        string? entityId,
        AuditAction action,
        string? oldValues = null,
        string? newValues = null,
        CancellationToken cancellationToken = default)
    {
        _context.AuditLogs.Add(new AuditLog
        {
            UserAccountId = _currentUser.UserAccountId,
            EntityName = entityName,
            EntityId = entityId,
            ActionName = action,
            OccurredAt = DateTime.UtcNow,
            OldValues = oldValues,
            NewValues = newValues
        });

        await _context.SaveChangesAsync(cancellationToken);
    }
}

using AlAtaaClinic.Domain.Common;
using AlAtaaClinic.Domain.Enums;
using AlAtaaClinic.Domain.Security;

namespace AlAtaaClinic.Domain.Audit;

public sealed class AuditLog : Entity<long>
{
    public long? UserAccountId { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public AuditAction ActionName { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? IpAddress { get; set; }

    public UserAccount? UserAccount { get; set; }
}

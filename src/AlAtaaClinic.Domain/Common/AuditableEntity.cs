namespace AlAtaaClinic.Domain.Common;

public abstract class AuditableEntity : Entity<long>
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

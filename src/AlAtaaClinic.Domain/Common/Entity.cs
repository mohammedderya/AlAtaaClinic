namespace AlAtaaClinic.Domain.Common;

public abstract class Entity<TKey> : IEntity<TKey>
{
    public TKey Id { get; set; } = default!;
}

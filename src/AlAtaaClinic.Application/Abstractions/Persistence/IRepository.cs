using System.Linq.Expressions;
using AlAtaaClinic.Application.Common.Models;
using AlAtaaClinic.Domain.Common;

namespace AlAtaaClinic.Application.Abstractions.Persistence;

public interface IRepository<TEntity>
    where TEntity : AggregateRoot
{
    Task<TEntity?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TEntity>> ListAsync(CancellationToken cancellationToken = default);
    Task<PagedResult<TEntity>> PageAsync(PageRequest request, CancellationToken cancellationToken = default);
    Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    void Update(TEntity entity);
    void Remove(TEntity entity);
}

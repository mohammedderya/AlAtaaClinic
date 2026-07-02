using System.Linq.Expressions;
using AlAtaaClinic.Application.Abstractions.Persistence;
using AlAtaaClinic.Application.Common.Models;
using AlAtaaClinic.Domain.Common;
using AlAtaaClinic.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AlAtaaClinic.Infrastructure.Repositories;

public class GenericRepository<TEntity> : IRepository<TEntity>
    where TEntity : AggregateRoot
{
    protected readonly ClinicDbContext DbContext;
    protected readonly DbSet<TEntity> DbSet;

    public GenericRepository(ClinicDbContext dbContext)
    {
        DbContext = dbContext;
        DbSet = dbContext.Set<TEntity>();
    }

    public virtual Task<TEntity?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return DbSet.FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
    }

    public virtual async Task<IReadOnlyList<TEntity>> ListAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking().ToListAsync(cancellationToken);
    }

    public virtual Task<PagedResult<TEntity>> PageAsync(PageRequest request, CancellationToken cancellationToken = default)
    {
        return DbSet.AsNoTracking().OrderBy(entity => entity.Id).ToPagedResultAsync(request, cancellationToken);
    }

    public Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return DbSet.AnyAsync(predicate, cancellationToken);
    }

    public Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        return DbSet.AddAsync(entity, cancellationToken).AsTask();
    }

    public void Update(TEntity entity)
    {
        DbSet.Update(entity);
    }

    public void Remove(TEntity entity)
    {
        DbSet.Remove(entity);
    }
}

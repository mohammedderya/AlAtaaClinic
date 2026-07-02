using AlAtaaClinic.Application.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore.Storage;

namespace AlAtaaClinic.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly ClinicDbContext _dbContext;

    public UnitOfWork(ClinicDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default)
    {
        await ExecuteInTransactionAsync<object?>(async token =>
        {
            await operation(token);
            return null;
        }, cancellationToken);
    }

    public async Task<TResult> ExecuteInTransactionAsync<TResult>(
        Func<CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        return await ExecuteAndCommitAsync(operation, transaction, cancellationToken);
    }

    private async Task<TResult> ExecuteAndCommitAsync<TResult>(
        Func<CancellationToken, Task<TResult>> operation,
        IDbContextTransaction transaction,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await operation(cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}

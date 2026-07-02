using AlAtaaClinic.Application.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace AlAtaaClinic.Infrastructure.Persistence;

internal static class RepositoryPagingExtensions
{
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        PageRequest request,
        CancellationToken cancellationToken)
    {
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip(request.Skip)
            .Take(request.NormalizedPageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<T>(items, totalCount, request.NormalizedPageNumber, request.NormalizedPageSize);
    }
}

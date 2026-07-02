using AlAtaaClinic.Application.Abstractions.Persistence;
using AlAtaaClinic.Application.Common.Models;
using AlAtaaClinic.Domain.Core;
using AlAtaaClinic.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AlAtaaClinic.Infrastructure.Repositories;

public sealed class DepartmentRepository : GenericRepository<Department>, IDepartmentRepository
{
    public DepartmentRepository(ClinicDbContext dbContext)
        : base(dbContext)
    {
    }

    public Task<PagedResult<Department>> SearchAsync(string? searchText, long? branchId, PageRequest request, CancellationToken cancellationToken = default)
    {
        IQueryable<Department> query = DbSet.AsNoTracking().Include(d => d.Branch);
        query = ApplySearch(query, searchText, branchId);
        return query.OrderBy(d => d.ArabicName).ToPagedResultAsync(request, cancellationToken);
    }

    public async Task<bool> IsNameUniqueAsync(long branchId, string arabicName, long? excludeId, CancellationToken cancellationToken = default)
    {
        return !await DbSet.AnyAsync(d =>
            d.BranchId == branchId &&
            d.ArabicName == arabicName &&
            (excludeId == null || d.Id != excludeId), cancellationToken);
    }

    private static IQueryable<Department> ApplySearch(IQueryable<Department> query, string? searchText, long? branchId)
    {
        if (branchId.HasValue)
        {
            query = query.Where(d => d.BranchId == branchId.Value);
        }

        if (string.IsNullOrWhiteSpace(searchText))
        {
            return query;
        }

        var term = searchText.Trim();
        return query.Where(d =>
            d.ArabicName.Contains(term) ||
            (d.EnglishName != null && d.EnglishName.Contains(term)));
    }
}

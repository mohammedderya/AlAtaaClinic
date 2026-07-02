using AlAtaaClinic.Application.Abstractions.Persistence;
using AlAtaaClinic.Application.Common.Models;
using AlAtaaClinic.Domain.Core;
using AlAtaaClinic.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AlAtaaClinic.Infrastructure.Repositories;

public sealed class BranchRepository : GenericRepository<Branch>, IBranchRepository
{
    public BranchRepository(ClinicDbContext dbContext)
        : base(dbContext)
    {
    }

    public Task<Branch?> GetByCodeAsync(string branchCode, CancellationToken cancellationToken = default)
    {
        return DbSet.FirstOrDefaultAsync(branch => branch.BranchCode == branchCode, cancellationToken);
    }

    public Task<PagedResult<Branch>> SearchAsync(string? searchText, PageRequest request, CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking();
        query = ApplySearch(query, searchText);
        return query.OrderBy(branch => branch.ArabicName).ToPagedResultAsync(request, cancellationToken);
    }

    private static IQueryable<Branch> ApplySearch(IQueryable<Branch> query, string? searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return query;
        }

        var term = searchText.Trim();
        return query.Where(branch =>
            branch.BranchCode.Contains(term) ||
            branch.ArabicName.Contains(term) ||
            (branch.EnglishName != null && branch.EnglishName.Contains(term)) ||
            (branch.PhoneNumber != null && branch.PhoneNumber.Contains(term)));
    }
}

using AlAtaaClinic.Application.Abstractions.Persistence;
using AlAtaaClinic.Application.Common.Models;
using AlAtaaClinic.Domain.Core;
using AlAtaaClinic.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AlAtaaClinic.Infrastructure.Repositories;

public sealed class StaffMemberRepository : GenericRepository<StaffMember>, IStaffMemberRepository
{
    public StaffMemberRepository(ClinicDbContext dbContext)
        : base(dbContext)
    {
    }

    public Task<StaffMember?> GetByCodeAsync(string staffCode, CancellationToken cancellationToken = default)
    {
        return DbSet.FirstOrDefaultAsync(s => s.StaffCode == staffCode, cancellationToken);
    }

    public Task<StaffMember?> GetWithDetailsAsync(long id, CancellationToken cancellationToken = default)
    {
        return DbSet.Include(s => s.Branch).Include(s => s.Department).FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public Task<PagedResult<StaffMember>> SearchAsync(string? searchText, PageRequest request, CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking();
        query = ApplySearch(query, searchText);
        return query.OrderBy(s => s.FullNameArabic).ToPagedResultAsync(request, cancellationToken);
    }

    public async Task<IReadOnlyList<StaffMember>> ListActiveAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking()
            .Where(s => s.IsActive)
            .OrderBy(s => s.FullNameArabic)
            .ToListAsync(cancellationToken);
    }

    private static IQueryable<StaffMember> ApplySearch(IQueryable<StaffMember> query, string? searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return query;
        }

        var term = searchText.Trim();
        return query.Where(s =>
            s.StaffCode.Contains(term) ||
            s.FullNameArabic.Contains(term) ||
            (s.FullNameEnglish != null && s.FullNameEnglish.Contains(term)) ||
            (s.PhoneNumber != null && s.PhoneNumber.Contains(term)) ||
            (s.JobTitle != null && s.JobTitle.Contains(term)));
    }
}

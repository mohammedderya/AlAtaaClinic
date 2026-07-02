using AlAtaaClinic.Application.Abstractions.Persistence;
using AlAtaaClinic.Application.Common.Models;
using AlAtaaClinic.Domain.Core;
using AlAtaaClinic.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AlAtaaClinic.Infrastructure.Repositories;

public sealed class DoctorRepository : IDoctorRepository
{
    private readonly ClinicDbContext _dbContext;
    private readonly DbSet<Doctor> _dbSet;

    public DoctorRepository(ClinicDbContext dbContext)
    {
        _dbContext = dbContext;
        _dbSet = dbContext.Set<Doctor>();
    }

    public Task<Doctor?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return _dbSet.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public Task<Doctor?> GetWithStaffAsync(long id, CancellationToken cancellationToken = default)
    {
        return _dbSet.Include(d => d.StaffMember).FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Doctor>> ListAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Doctor>> ListActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking()
            .Where(d => d.IsActive)
            .OrderBy(d => d.Specialty)
            .ToListAsync(cancellationToken);
    }

    public Task<PagedResult<Doctor>> SearchAsync(string? searchText, PageRequest request, CancellationToken cancellationToken = default)
    {
        IQueryable<Doctor> query = _dbSet.AsNoTracking().Include(d => d.StaffMember);
        query = ApplySearch(query, searchText);
        return query.OrderBy(d => d.Specialty).ToPagedResultAsync(request, cancellationToken);
    }

    public Task<Doctor?> GetByStaffMemberIdAsync(long staffMemberId, CancellationToken cancellationToken = default)
    {
        return _dbSet.FirstOrDefaultAsync(d => d.StaffMemberId == staffMemberId, cancellationToken);
    }

    public Task AddAsync(Doctor entity, CancellationToken cancellationToken = default)
    {
        return _dbSet.AddAsync(entity, cancellationToken).AsTask();
    }

    public void Update(Doctor entity)
    {
        _dbSet.Update(entity);
    }

    public void Remove(Doctor entity)
    {
        _dbSet.Remove(entity);
    }

    private static IQueryable<Doctor> ApplySearch(IQueryable<Doctor> query, string? searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return query;
        }

        var term = searchText.Trim();
        return query.Where(d =>
            d.Specialty.Contains(term) ||
            (d.LicenseNumber != null && d.LicenseNumber.Contains(term)) ||
            (d.StaffMember != null && (
                d.StaffMember.FullNameArabic.Contains(term) ||
                (d.StaffMember.FullNameEnglish != null && d.StaffMember.FullNameEnglish.Contains(term)))));
    }
}

using AlAtaaClinic.Application.Abstractions.Persistence;
using AlAtaaClinic.Application.Common.Models;
using AlAtaaClinic.Domain.Core;
using AlAtaaClinic.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AlAtaaClinic.Infrastructure.Repositories;

public sealed class PatientRepository : GenericRepository<Patient>, IPatientRepository
{
    public PatientRepository(ClinicDbContext dbContext)
        : base(dbContext)
    {
    }

    public Task<Patient?> GetByFileNumberAsync(string fileNumber, CancellationToken cancellationToken = default)
    {
        return DbSet.FirstOrDefaultAsync(patient => patient.FileNumber == fileNumber, cancellationToken);
    }

    public Task<PagedResult<Patient>> SearchAsync(string? searchText, PageRequest request, CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking();
        query = ApplySearch(query, searchText);
        return query.OrderBy(patient => patient.FullNameArabic).ToPagedResultAsync(request, cancellationToken);
    }

    private static IQueryable<Patient> ApplySearch(IQueryable<Patient> query, string? searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return query;
        }

        var term = searchText.Trim();
        return query.Where(patient =>
            patient.FileNumber.Contains(term) ||
            patient.FullNameArabic.Contains(term) ||
            (patient.NationalId != null && patient.NationalId.Contains(term)) ||
            (patient.PhoneNumber != null && patient.PhoneNumber.Contains(term)));
    }
}

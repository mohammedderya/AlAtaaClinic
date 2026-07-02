using AlAtaaClinic.Application.Abstractions.Persistence;
using AlAtaaClinic.Application.Common.Models;
using AlAtaaClinic.Domain.Charity;
using AlAtaaClinic.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AlAtaaClinic.Infrastructure.Repositories;

public sealed class CharityCaseRepository : GenericRepository<CharityCase>, ICharityCaseRepository
{
    public CharityCaseRepository(ClinicDbContext dbContext)
        : base(dbContext)
    {
    }

    public Task<PagedResult<CharityCase>> SearchAsync(long? patientId, string? caseNumber, PageRequest request, CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking();
        if (patientId.HasValue) query = query.Where(charityCase => charityCase.PatientId == patientId);
        if (!string.IsNullOrWhiteSpace(caseNumber)) query = query.Where(charityCase => charityCase.CaseNumber.Contains(caseNumber));
        return query.OrderByDescending(charityCase => charityCase.CreatedAt).ToPagedResultAsync(request, cancellationToken);
    }
}

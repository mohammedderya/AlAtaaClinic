using AlAtaaClinic.Application.Abstractions.Persistence;
using AlAtaaClinic.Application.Common.Models;
using AlAtaaClinic.Domain.Clinical;
using AlAtaaClinic.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AlAtaaClinic.Infrastructure.Repositories;

public sealed class VisitRepository : GenericRepository<Visit>, IVisitRepository
{
    public VisitRepository(ClinicDbContext dbContext)
        : base(dbContext)
    {
    }

    public override Task<Visit?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return GetWithDetailsAsync(id, cancellationToken);
    }

    public Task<Visit?> GetWithDetailsAsync(long id, CancellationToken cancellationToken = default)
    {
        return WithDetails().FirstOrDefaultAsync(visit => visit.Id == id, cancellationToken);
    }

    public Task<PagedResult<Visit>> SearchAsync(
        long? patientId,
        long? doctorId,
        DateTime? from,
        DateTime? to,
        PageRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking();
        if (patientId.HasValue) query = query.Where(visit => visit.PatientId == patientId);
        if (doctorId.HasValue) query = query.Where(visit => visit.DoctorId == doctorId);
        if (from.HasValue) query = query.Where(visit => visit.VisitDate >= from);
        if (to.HasValue) query = query.Where(visit => visit.VisitDate <= to);
        return query.OrderByDescending(visit => visit.VisitDate).ToPagedResultAsync(request, cancellationToken);
    }

    private IQueryable<Visit> WithDetails()
    {
        return DbSet
            .Include(visit => visit.VitalSigns)
            .Include(visit => visit.Diagnoses)
            .Include(visit => visit.Procedures)
            .Include(visit => visit.Prescriptions).ThenInclude(prescription => prescription.Items)
            .Include(visit => visit.MedicalOrders).ThenInclude(order => order.Results);
    }
}

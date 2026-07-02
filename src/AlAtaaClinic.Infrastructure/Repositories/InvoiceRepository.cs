using AlAtaaClinic.Application.Abstractions.Persistence;
using AlAtaaClinic.Application.Common.Models;
using AlAtaaClinic.Domain.Billing;
using AlAtaaClinic.Domain.Enums;
using AlAtaaClinic.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AlAtaaClinic.Infrastructure.Repositories;

public sealed class InvoiceRepository : GenericRepository<Invoice>, IInvoiceRepository
{
    public InvoiceRepository(ClinicDbContext dbContext)
        : base(dbContext)
    {
    }

    public override Task<Invoice?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return GetWithDetailsAsync(id, cancellationToken);
    }

    public Task<Invoice?> GetWithDetailsAsync(long id, CancellationToken cancellationToken = default)
    {
        return WithDetails().FirstOrDefaultAsync(invoice => invoice.Id == id, cancellationToken);
    }

    public Task<PagedResult<Invoice>> SearchAsync(long? patientId, string? status, PageRequest request, CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking();
        if (patientId.HasValue) query = query.Where(invoice => invoice.PatientId == patientId);
        if (Enum.TryParse<InvoiceStatus>(status, true, out var invoiceStatus))
        {
            query = query.Where(invoice => invoice.Status == invoiceStatus);
        }

        return query.OrderByDescending(invoice => invoice.CreatedAt).ToPagedResultAsync(request, cancellationToken);
    }

    private IQueryable<Invoice> WithDetails()
    {
        return DbSet
            .Include(invoice => invoice.Lines)
            .Include(invoice => invoice.Payments)
            .Include(invoice => invoice.DiscountApprovals);
    }
}

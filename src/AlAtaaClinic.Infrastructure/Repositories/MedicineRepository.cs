using AlAtaaClinic.Application.Abstractions.Persistence;
using AlAtaaClinic.Application.Common.Models;
using AlAtaaClinic.Domain.Inventory;
using AlAtaaClinic.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AlAtaaClinic.Infrastructure.Repositories;

public sealed class MedicineRepository : GenericRepository<Medicine>, IMedicineRepository
{
    public MedicineRepository(ClinicDbContext dbContext)
        : base(dbContext)
    {
    }

    public override Task<Medicine?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return DbSet
            .Include(medicine => medicine.StockBatches)
            .FirstOrDefaultAsync(medicine => medicine.Id == id, cancellationToken);
    }

    public Task<Medicine?> GetByCodeAsync(string medicineCode, CancellationToken cancellationToken = default)
    {
        return DbSet.FirstOrDefaultAsync(medicine => medicine.MedicineCode == medicineCode, cancellationToken);
    }

    public Task<PagedResult<Medicine>> SearchAsync(string? searchText, PageRequest request, CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking();
        query = ApplySearch(query, searchText);
        return query.OrderBy(medicine => medicine.GenericName).ToPagedResultAsync(request, cancellationToken);
    }

    private static IQueryable<Medicine> ApplySearch(IQueryable<Medicine> query, string? searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return query;
        }

        var term = searchText.Trim();
        return query.Where(medicine =>
            medicine.MedicineCode.Contains(term) ||
            medicine.GenericName.Contains(term) ||
            (medicine.TradeName != null && medicine.TradeName.Contains(term)));
    }
}

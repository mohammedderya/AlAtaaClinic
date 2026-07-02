using AlAtaaClinic.Application.Abstractions.Persistence;
using AlAtaaClinic.Application.Common.Models;
using AlAtaaClinic.Domain.Clinical;
using AlAtaaClinic.Infrastructure.Persistence;

namespace AlAtaaClinic.Infrastructure.Repositories;

public sealed class AppointmentRepository : GenericRepository<Appointment>, IAppointmentRepository
{
    public AppointmentRepository(ClinicDbContext dbContext)
        : base(dbContext)
    {
    }

    public Task<PagedResult<Appointment>> SearchAsync(
        long? doctorId,
        DateTime? from,
        DateTime? to,
        PageRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsQueryable();
        if (doctorId.HasValue) query = query.Where(appointment => appointment.DoctorId == doctorId);
        if (from.HasValue) query = query.Where(appointment => appointment.AppointmentStart >= from);
        if (to.HasValue) query = query.Where(appointment => appointment.AppointmentStart <= to);
        return query.OrderBy(appointment => appointment.AppointmentStart).ToPagedResultAsync(request, cancellationToken);
    }
}

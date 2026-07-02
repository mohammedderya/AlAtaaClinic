using AlAtaaClinic.Application.Abstractions.Persistence;
using AlAtaaClinic.Domain.Security;
using AlAtaaClinic.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AlAtaaClinic.Infrastructure.Repositories;

public sealed class UserAccountRepository : GenericRepository<UserAccount>, IUserAccountRepository
{
    public UserAccountRepository(ClinicDbContext dbContext)
        : base(dbContext)
    {
    }

    public override Task<UserAccount?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return DbSet
            .Include(user => user.StaffMember)
            .FirstOrDefaultAsync(user => user.Id == id, cancellationToken);
    }

    public Task<UserAccount?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return WithPermissions()
            .FirstOrDefaultAsync(user => user.Username == username, cancellationToken);
    }

    public Task<UserAccount?> GetWithPermissionsAsync(long userAccountId, CancellationToken cancellationToken = default)
    {
        return WithPermissions()
            .FirstOrDefaultAsync(user => user.Id == userAccountId, cancellationToken);
    }

    public Task<int> CountActiveAsync(CancellationToken cancellationToken = default)
    {
        return DbSet.CountAsync(user => user.IsActive, cancellationToken);
    }

    private IQueryable<UserAccount> WithPermissions()
    {
        return DbSet
            .Include(user => user.StaffMember)
            .Include(user => user.UserRoles)
                .ThenInclude(userRole => userRole.Role)
                    .ThenInclude(role => role!.RolePermissions)
                        .ThenInclude(rolePermission => rolePermission.Permission);
    }
}

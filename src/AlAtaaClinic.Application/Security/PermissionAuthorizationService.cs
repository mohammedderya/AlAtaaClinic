using AlAtaaClinic.Application.Abstractions.Persistence;
using AlAtaaClinic.Application.Abstractions.Security;
using AlAtaaClinic.Application.Common.Exceptions;
using AlAtaaClinic.Domain.Security;

namespace AlAtaaClinic.Application.Security;

public sealed class PermissionAuthorizationService : IAuthorizationService
{
    private readonly ICurrentUserService _currentUser;
    private readonly IUserAccountRepository _users;

    public PermissionAuthorizationService(ICurrentUserService currentUser, IUserAccountRepository users)
    {
        _currentUser = currentUser;
        _users = users;
    }

    public async Task EnsurePermissionAsync(string permission, CancellationToken cancellationToken = default)
    {
        if (!await HasPermissionAsync(permission, cancellationToken))
        {
            throw new ForbiddenException(permission);
        }
    }

    public async Task<bool> HasPermissionAsync(string permission, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserAccountId is null)
        {
            return false;
        }

        var user = await _users.GetWithPermissionsAsync(_currentUser.UserAccountId.Value, cancellationToken);
        return user is not null && HasPermission(user, permission);
    }

    private static bool HasPermission(UserAccount user, string permission)
    {
        return user.UserRoles
            .SelectMany(userRole => userRole.Role?.RolePermissions ?? [])
            .Any(rolePermission => rolePermission.Permission?.PermissionKey == permission);
    }
}

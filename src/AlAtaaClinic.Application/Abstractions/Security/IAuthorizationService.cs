namespace AlAtaaClinic.Application.Abstractions.Security;

public interface IAuthorizationService
{
    Task EnsurePermissionAsync(string permission, CancellationToken cancellationToken = default);
    Task<bool> HasPermissionAsync(string permission, CancellationToken cancellationToken = default);
}

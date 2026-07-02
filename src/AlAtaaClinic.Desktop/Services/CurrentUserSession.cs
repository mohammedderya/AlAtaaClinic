using AlAtaaClinic.Application.Abstractions.Security;

namespace AlAtaaClinic.Desktop.Services;

public sealed class CurrentUserSession : ICurrentUserService
{
    private readonly HashSet<string> _permissions = new(StringComparer.OrdinalIgnoreCase);

    public long? UserAccountId { get; private set; }
    public string? Username { get; private set; }
    public string? FullNameArabic { get; private set; }
    public bool IsAuthenticated => UserAccountId.HasValue;

    public IReadOnlyCollection<string> Permissions => _permissions;

    public event EventHandler? SignedIn;

    public void SignIn(long userAccountId, string username, string fullNameArabic, IEnumerable<string> permissions)
    {
        UserAccountId = userAccountId;
        Username = username;
        FullNameArabic = fullNameArabic;
        _permissions.Clear();
        foreach (var permission in permissions)
        {
            _permissions.Add(permission);
        }
        SignedIn?.Invoke(this, EventArgs.Empty);
    }

    public bool HasPermission(string permission)
    {
        return _permissions.Contains(permission);
    }

    public void SignOut()
    {
        UserAccountId = null;
        Username = null;
        FullNameArabic = null;
        _permissions.Clear();
    }
}

namespace AlAtaaClinic.Application.Abstractions.Security;

public interface ICurrentUserService
{
    long? UserAccountId { get; }
    string? Username { get; }
    bool IsAuthenticated { get; }
}

namespace AlAtaaClinic.Application.Abstractions.Security;

public interface ILicenseProvider
{
    bool IsValid { get; }
    int MaxUsers { get; }
}

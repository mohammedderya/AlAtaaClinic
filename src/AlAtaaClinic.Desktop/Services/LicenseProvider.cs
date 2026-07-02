using AlAtaaClinic.Application.Abstractions.Security;

namespace AlAtaaClinic.Desktop.Services;

public sealed class LicenseProvider : ILicenseProvider
{
    private readonly LicenseService _licenseService;

    public LicenseProvider(LicenseService licenseService)
    {
        _licenseService = licenseService;
    }

    public bool IsValid => _licenseService.IsValid;
    public int MaxUsers => _licenseService.CurrentLicense.MaxUsers;
}

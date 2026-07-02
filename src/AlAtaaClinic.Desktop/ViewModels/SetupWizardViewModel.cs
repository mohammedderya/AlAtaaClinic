using System.Windows.Input;
using AlAtaaClinic.Desktop.Services;
using AlAtaaClinic.Desktop.Common;

namespace AlAtaaClinic.Desktop.ViewModels;

public sealed class SetupWizardViewModel : DialogViewModelBase
{
    private readonly TenantService _tenantService;
    private readonly LicenseService _licenseService;

    public SetupWizardViewModel(
        TenantService tenantService,
        LicenseService licenseService,
        Application.Common.ExceptionHandling.IExceptionHandler exceptionHandler)
        : base(exceptionHandler)
    {
        _tenantService = tenantService;
        _licenseService = licenseService;
        NextCommand = new RelayCommand(_ => Next());
        BackCommand = new RelayCommand(_ => Back());
    }

    private int _currentStep;
    public int CurrentStep
    {
        get => _currentStep;
        set
        {
            SetProperty(ref _currentStep, value);
            OnPropertyChanged(nameof(IsStep1));
            OnPropertyChanged(nameof(IsStep2));
            OnPropertyChanged(nameof(IsStep3));
            OnPropertyChanged(nameof(IsStep4));
            OnPropertyChanged(nameof(CanGoBack));
            OnPropertyChanged(nameof(NextButtonText));
            OnPropertyChanged(nameof(StepTitle));
        }
    }

    public bool IsStep1 => CurrentStep == 0;
    public bool IsStep2 => CurrentStep == 1;
    public bool IsStep3 => CurrentStep == 2;
    public bool IsStep4 => CurrentStep == 3;
    public bool CanGoBack => CurrentStep > 0;
    public string NextButtonText => CurrentStep == 3 ? "Finish" : "Next";
    public string StepTitle => CurrentStep switch
    {
        0 => "Step 1: Clinic Information",
        1 => "Step 2: Database Connection",
        2 => "Step 3: License Activation",
        3 => "Step 4: Complete",
        _ => ""
    };

    private string _clinicName = string.Empty;
    public string ClinicName { get => _clinicName; set => SetProperty(ref _clinicName, value); }

    private string _serverName = ".";
    public string ServerName { get => _serverName; set => SetProperty(ref _serverName, value); }

    private string _databaseName = "AlAtaaClinic";
    public string DatabaseName { get => _databaseName; set => SetProperty(ref _databaseName, value); }

    private bool _isWindowsAuth = true;
    public bool IsWindowsAuth
    {
        get => _isWindowsAuth;
        set
        {
            SetProperty(ref _isWindowsAuth, value);
            if (value) SetProperty(ref _isSqlAuth, false, nameof(IsSqlAuth));
            OnPropertyChanged(nameof(ShowSqlCredentials));
        }
    }

    private bool _isSqlAuth;
    public bool IsSqlAuth
    {
        get => _isSqlAuth;
        set
        {
            SetProperty(ref _isSqlAuth, value);
            if (value && _isWindowsAuth) SetProperty(ref _isWindowsAuth, false, nameof(IsWindowsAuth));
            if (value && (ServerName is "." or "localhost" or "127.0.0.1" && !ServerName.Contains(',')))
                ServerName = "localhost,1433";
            OnPropertyChanged(nameof(ShowSqlCredentials));
        }
    }

    public bool ShowSqlCredentials => IsSqlAuth;

    private string _sqlUsername = "sa";
    public string SqlUsername { get => _sqlUsername; set => SetProperty(ref _sqlUsername, value); }

    private string _sqlPassword = "YourStrong!Pass1";
    public string SqlPassword { get => _sqlPassword; set => SetProperty(ref _sqlPassword, value); }

    private string _licenseKey = string.Empty;
    public string LicenseKey { get => _licenseKey; set => SetProperty(ref _licenseKey, value); }

    private string _setupStatusMessage = string.Empty;
    public new string StatusMessage { get => _setupStatusMessage; set => SetProperty(ref _setupStatusMessage, value); }

    private bool _isSuccess;
    public bool IsSuccess { get => _isSuccess; set => SetProperty(ref _isSuccess, value); }

    public bool SetupCompleted { get; private set; }

    public ICommand NextCommand { get; }
    public ICommand BackCommand { get; }

    private void ApplyDatabaseSettings()
    {
        _tenantService.ServerName = ServerName;
        _tenantService.DatabaseName = DatabaseName;
        _tenantService.ClinicianName = ClinicName;
        _tenantService.UseWindowsAuth = IsWindowsAuth;
        _tenantService.SqlUsername = SqlUsername;
        _tenantService.SqlPassword = SqlPassword;
        _tenantService.NormalizeConnectionSettings();
        ServerName = _tenantService.ServerName;
    }

    protected override Task SaveAsync()
    {
        if (!TryFinishSetup(out var errorMessage, out var returnStep))
        {
            StatusMessage = errorMessage;
            if (returnStep.HasValue)
            {
                CurrentStep = returnStep.Value;
            }

            IsSuccess = false;
            return Task.CompletedTask;
        }

        CompleteSetup();
        return Task.CompletedTask;
    }

    private void FinishSetup()
    {
        if (!TryFinishSetup(out var errorMessage, out var returnStep))
        {
            StatusMessage = errorMessage;
            if (returnStep.HasValue)
            {
                CurrentStep = returnStep.Value;
            }

            return;
        }

        CompleteSetup();
    }

    private void CompleteSetup()
    {
        SetupCompleted = true;
        IsSuccess = true;
        Close(true);
    }

    private bool TryFinishSetup(out string errorMessage, out int? returnStep)
    {
        errorMessage = string.Empty;
        returnStep = null;

        ApplyDatabaseSettings();

        var (connected, connectionError) = _tenantService.TestConnectionWithDetails();
        if (!connected)
        {
            errorMessage = $"Connection failed: {connectionError}";
            returnStep = 1;
            return false;
        }

        try
        {
            _tenantService.Save();
        }
        catch (Exception ex)
        {
            errorMessage = $"Could not save settings: {ex.Message}";
            return false;
        }

        if (!string.IsNullOrWhiteSpace(LicenseKey) && !_licenseService.Activate(LicenseKey))
        {
            errorMessage = "Invalid or expired license key.";
            returnStep = 2;
            return false;
        }

        return true;
    }

    private void Next()
    {
        switch (CurrentStep)
        {
            case 0:
                if (string.IsNullOrWhiteSpace(ClinicName))
                {
                    StatusMessage = "Please enter clinic name.";
                    return;
                }
                StatusMessage = string.Empty;
                CurrentStep = 1;
                break;
            case 1:
                if (string.IsNullOrWhiteSpace(ServerName) || string.IsNullOrWhiteSpace(DatabaseName))
                {
                    StatusMessage = "Please fill all fields.";
                    return;
                }
                if (IsSqlAuth && (string.IsNullOrWhiteSpace(SqlUsername) || string.IsNullOrWhiteSpace(SqlPassword)))
                {
                    StatusMessage = "Please enter SQL credentials.";
                    return;
                }

                ApplyDatabaseSettings();
                var (connected, connectionError) = _tenantService.TestConnectionWithDetails();
                if (!connected)
                {
                    StatusMessage = $"Connection failed: {connectionError}";
                    return;
                }

                var (databaseReady, databaseError) = _tenantService.EnsureDatabaseExists();
                if (!databaseReady)
                {
                    StatusMessage = $"Database setup failed: {databaseError}";
                    return;
                }

                StatusMessage = string.Empty;
                CurrentStep = 2;
                break;
            case 2:
                StatusMessage = string.Empty;
                CurrentStep = 3;
                break;
            case 3:
                FinishSetup();
                break;
        }
    }

    private void Back()
    {
        if (CurrentStep > 0) CurrentStep--;
    }
}

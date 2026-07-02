using System.Collections.ObjectModel;
using System.Windows.Input;
using AlAtaaClinic.Application.Abstractions.Security;
using AlAtaaClinic.Desktop.Common;
using AlAtaaClinic.Desktop.Services;

namespace AlAtaaClinic.Desktop.ViewModels;

public sealed class ShellViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
    private readonly CurrentUserSession _session;
    private readonly IThemeService _themeService;
    private readonly TranslationService _translation;
    private object? _currentViewModel;
    private NavigationItem? _selectedNavigationItem;

    public ShellViewModel(
        INavigationService navigationService,
        CurrentUserSession session,
        IThemeService themeService,
        TranslationService translation)
    {
        _navigationService = navigationService;
        _session = session;
        _themeService = themeService;
        _translation = translation;
        NavigationItems = new ObservableCollection<NavigationItem>();
        NavigateCommand = new RelayCommand(item => Navigate((NavigationItem)item!));
        ToggleThemeCommand = new RelayCommand(_ => _themeService.ToggleTheme());
        SwitchLanguageCommand = new RelayCommand(_ => SwitchLanguage());
        _navigationService.Navigated += (_, viewModel) => CurrentViewModel = viewModel;
        _session.SignedIn += OnSignedIn;
        _translation.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == "Item[]") RebuildNavigation();
        };
        RebuildNavigation();
    }

    public ObservableCollection<NavigationItem> NavigationItems { get; }
    public ICommand NavigateCommand { get; }
    public ICommand ToggleThemeCommand { get; }
    public ICommand SwitchLanguageCommand { get; }
    public string CurrentUserDisplay => _session.FullNameArabic ?? _session.Username ?? "User";
    public string CurrentUserInitial => (CurrentUserDisplay.Length > 0 ? CurrentUserDisplay[..1] : "U").ToUpper();

    public string CurrentPageTitle => SelectedNavigationItem?.Title ?? "";

    public object? CurrentViewModel
    {
        get => _currentViewModel;
        private set => SetProperty(ref _currentViewModel, value);
    }

    public NavigationItem? SelectedNavigationItem
    {
        get => _selectedNavigationItem;
        set
        {
            if (SetProperty(ref _selectedNavigationItem, value))
                OnPropertyChanged(nameof(CurrentPageTitle));
        }
    }

    private void SwitchLanguage()
    {
        var current = _translation.CurrentCulture.TwoLetterISOLanguageName;
        var next = current == "ar" ? "en" : "ar";
        _translation.SetLanguage(next);
    }

    private void OnSignedIn(object? sender, EventArgs e)
    {
        RebuildNavigation();
        OnPropertyChanged(nameof(CurrentUserDisplay));
    }

    private void Navigate(NavigationItem item)
    {
        SelectedNavigationItem = item;
        _navigationService.NavigateTo(item.Key);
    }

    private void RebuildNavigation()
    {
        NavigationItems.Clear();
        foreach (var item in BuildNavigation())
        {
            NavigationItems.Add(item);
        }

        if (SelectedNavigationItem is null && NavigationItems.Count > 0)
        {
            SelectedNavigationItem = NavigationItems[0];
            Navigate(SelectedNavigationItem);
        }
    }

    private IEnumerable<NavigationItem> BuildNavigation()
    {
        var t = _translation;
        var items = new[]
        {
            new NavigationItem(NavigationKey.Dashboard, t["Nav.Dashboard"], "▦", string.Empty),
            new NavigationItem(NavigationKey.Branches, t["Nav.Branches"], "B", PermissionKeys.StaffRead),
            new NavigationItem(NavigationKey.Departments, t["Nav.Departments"], "D", PermissionKeys.StaffRead),
            new NavigationItem(NavigationKey.Patients, t["Nav.Patients"], "P", PermissionKeys.PatientsRead),
            new NavigationItem(NavigationKey.StaffMembers, t["Nav.StaffMembers"], "F", PermissionKeys.StaffRead),
            new NavigationItem(NavigationKey.Doctors, t["Nav.Doctors"], "T", PermissionKeys.StaffRead),
            new NavigationItem(NavigationKey.PatientHistory, t["Nav.History"], "H", PermissionKeys.ClinicalRead),
            new NavigationItem(NavigationKey.Appointments, t["Nav.Appointments"], "A", PermissionKeys.SchedulingRead),
            new NavigationItem(NavigationKey.Visits, t["Nav.Visits"], "C", PermissionKeys.ClinicalRead),
            new NavigationItem(NavigationKey.Billing, t["Nav.Billing"], "$", PermissionKeys.BillingRead),
            new NavigationItem(NavigationKey.Charity, t["Nav.Charity"], "%", PermissionKeys.CharityRead),
            new NavigationItem(NavigationKey.Inventory, t["Nav.Inventory"], "M", PermissionKeys.InventoryRead),
            new NavigationItem(NavigationKey.Reports, t["Nav.Reports"], "R", PermissionKeys.BillingRead),
            new NavigationItem(NavigationKey.Settings, t["Nav.Settings"], "S", string.Empty)
        };

        return items.Where(item => string.IsNullOrWhiteSpace(item.Permission) || _session.HasPermission(item.Permission));
    }
}

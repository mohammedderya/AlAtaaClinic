namespace AlAtaaClinic.Desktop.Services;

public sealed record NavigationItem(
    NavigationKey Key,
    string Title,
    string Icon,
    string Permission);

namespace AlAtaaClinic.Application.Abstractions.Security;

public static class PermissionKeys
{
    public const string PatientsRead = "patients.read";
    public const string PatientsWrite = "patients.write";
    public const string SchedulingRead = "scheduling.read";
    public const string SchedulingWrite = "scheduling.write";
    public const string ClinicalRead = "clinical.read";
    public const string ClinicalWrite = "clinical.write";
    public const string BillingRead = "billing.read";
    public const string BillingWrite = "billing.write";
    public const string CharityRead = "charity.read";
    public const string CharityWrite = "charity.write";
    public const string InventoryRead = "inventory.read";
    public const string InventoryWrite = "inventory.write";
    public const string StaffRead = "staff.read";
    public const string StaffWrite = "staff.write";
    public const string UsersRead = "users.read";
    public const string UsersWrite = "users.write";
    public const string AuditRead = "audit.read";

    public static IReadOnlyList<string> All { get; } =
    [
        PatientsRead, PatientsWrite,
        SchedulingRead, SchedulingWrite,
        ClinicalRead, ClinicalWrite,
        BillingRead, BillingWrite,
        CharityRead, CharityWrite,
        InventoryRead, InventoryWrite,
        StaffRead, StaffWrite,
        UsersRead, UsersWrite,
        AuditRead
    ];
}

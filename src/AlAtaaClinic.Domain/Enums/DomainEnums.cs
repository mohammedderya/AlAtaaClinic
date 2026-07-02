namespace AlAtaaClinic.Domain.Enums;

public enum Gender
{
    Male = 1,
    Female = 2
}

public enum AppointmentStatus
{
    Scheduled = 1,
    CheckedIn = 2,
    Completed = 3,
    Cancelled = 4,
    NoShow = 5
}

public enum VisitStatus
{
    Open = 1,
    Closed = 2,
    Cancelled = 3
}

public enum MedicalOrderType
{
    Lab = 1,
    Radiology = 2,
    Other = 3
}

public enum MedicalOrderStatus
{
    Ordered = 1,
    InProgress = 2,
    Completed = 3,
    Cancelled = 4
}

public enum InvoiceStatus
{
    Draft = 1,
    Issued = 2,
    PartiallyPaid = 3,
    Paid = 4,
    Voided = 5
}

public enum PaymentMethod
{
    Cash = 1,
    Card = 2,
    BankTransfer = 3,
    CharityCoverage = 4
}

public enum DiscountStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3,
    Cancelled = 4
}

public enum EligibilityStatus
{
    Pending = 1,
    Eligible = 2,
    NotEligible = 3,
    Expired = 4,
    Suspended = 5
}

public enum StockMovementType
{
    Receive = 1,
    Dispense = 2,
    Adjustment = 3,
    Return = 4,
    Expire = 5
}

public enum AuditAction
{
    Create = 1,
    Update = 2,
    Delete = 3,
    Login = 4,
    Access = 5,
    Void = 6
}

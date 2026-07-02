namespace AlAtaaClinic.Application.Features.Medicines;

public sealed record MedicineDto(
    long Id,
    string MedicineCode,
    string GenericName,
    string? TradeName,
    string Unit,
    bool IsActive,
    decimal QuantityOnHand);

public sealed record MedicineListDto(
    long Id,
    string MedicineCode,
    string GenericName,
    string? TradeName,
    string Unit,
    bool IsActive);

using AlAtaaClinic.Application.Common.Messaging;
using AlAtaaClinic.Application.Common.Models;

namespace AlAtaaClinic.Application.Features.Medicines;

public sealed record CreateMedicineCommand(
    string MedicineCode,
    string GenericName,
    string? TradeName,
    string Unit) : ICommand<MedicineDto>;

public sealed record UpdateMedicineCommand(
    long Id,
    string GenericName,
    string? TradeName,
    string Unit,
    bool IsActive) : ICommand<MedicineDto>;

public sealed record DeleteMedicineCommand(long Id) : ICommand<OperationResult>;

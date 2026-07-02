using AlAtaaClinic.Application.Common.Messaging;
using AlAtaaClinic.Application.Common.Models;

namespace AlAtaaClinic.Application.Features.Branches;

public sealed record CreateBranchCommand(
    string BranchCode,
    string ArabicName,
    string? EnglishName,
    string? PhoneNumber,
    string? AddressLine) : ICommand<BranchDto>;

public sealed record UpdateBranchCommand(
    long Id,
    string ArabicName,
    string? EnglishName,
    string? PhoneNumber,
    string? AddressLine,
    bool IsActive) : ICommand<BranchDto>;

public sealed record DeleteBranchCommand(long Id) : ICommand<OperationResult>;

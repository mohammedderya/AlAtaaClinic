using AlAtaaClinic.Application.Common.Messaging;
using AlAtaaClinic.Application.Common.Models;

namespace AlAtaaClinic.Application.Features.Departments;

public sealed record CreateDepartmentCommand(
    long BranchId,
    string ArabicName,
    string? EnglishName) : ICommand<DepartmentDto>;

public sealed record UpdateDepartmentCommand(
    long Id,
    long BranchId,
    string ArabicName,
    string? EnglishName,
    bool IsActive) : ICommand<DepartmentDto>;

public sealed record DeleteDepartmentCommand(long Id) : ICommand<OperationResult>;

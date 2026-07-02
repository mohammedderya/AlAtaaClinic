using AlAtaaClinic.Application.Common.Messaging;
using AlAtaaClinic.Application.Common.Models;

namespace AlAtaaClinic.Application.Features.Auth;

public sealed record LoginCommand(string Username, string Password) : ICommand<LoginResultDto>;

public sealed record CreateUserAccountCommand(
    long StaffMemberId,
    string Username,
    string Password) : ICommand<UserAccountDto>;

public sealed record UpdateUserAccountStatusCommand(long Id, bool IsActive) : ICommand<UserAccountDto>;

public sealed record DeleteUserAccountCommand(long Id) : ICommand<OperationResult>;

public sealed record ChangePasswordCommand(
    long UserAccountId,
    string CurrentPassword,
    string NewPassword) : ICommand<OperationResult>;

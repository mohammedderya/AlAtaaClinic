using AlAtaaClinic.Application.Common.Models;

namespace AlAtaaClinic.Application.Features.Auth;

public interface IAuthService
{
    Task<LoginResultDto> LoginAsync(LoginCommand command, CancellationToken cancellationToken = default);
    Task<OperationResult> ChangePasswordAsync(ChangePasswordCommand command, CancellationToken cancellationToken = default);
    Task<UserAccountDto> CreateUserAsync(CreateUserAccountCommand command, CancellationToken cancellationToken = default);
    Task<UserAccountDto> UpdateStatusAsync(UpdateUserAccountStatusCommand command, CancellationToken cancellationToken = default);
    Task<OperationResult> DeleteAsync(DeleteUserAccountCommand command, CancellationToken cancellationToken = default);
    Task<UserAccountDto> GetByIdAsync(GetUserAccountByIdQuery query, CancellationToken cancellationToken = default);
}

using AlAtaaClinic.Application.Abstractions.Audit;
using AlAtaaClinic.Application.Abstractions.Logging;
using AlAtaaClinic.Application.Abstractions.Persistence;
using AlAtaaClinic.Application.Abstractions.Security;
using AlAtaaClinic.Application.Common.Exceptions;
using AlAtaaClinic.Application.Common.Messaging;
using AlAtaaClinic.Application.Common.Models;
using AlAtaaClinic.Application.Common.Validation;
using AlAtaaClinic.Domain.Enums;
using AlAtaaClinic.Domain.Security;

namespace AlAtaaClinic.Application.Features.Auth;

public sealed class AuthService :
    IAuthService,
    ICommandHandler<LoginCommand, LoginResultDto>,
    ICommandHandler<ChangePasswordCommand, OperationResult>,
    ICommandHandler<CreateUserAccountCommand, UserAccountDto>,
    ICommandHandler<UpdateUserAccountStatusCommand, UserAccountDto>,
    ICommandHandler<DeleteUserAccountCommand, OperationResult>,
    IQueryHandler<GetUserAccountByIdQuery, UserAccountDto>
{
    private readonly IUserAccountRepository _users;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuthorizationService _authorization;
    private readonly ILicenseProvider _license;
    private readonly IAuditService _audit;
    private readonly ValidationRunner<LoginCommand> _loginValidator;
    private readonly ValidationRunner<CreateUserAccountCommand> _createValidator;
    private readonly ValidationRunner<ChangePasswordCommand> _changePasswordValidator;
    private readonly IAppLogger<AuthService> _logger;

    public AuthService(
        IUserAccountRepository users,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IAuthorizationService authorization,
        ILicenseProvider license,
        IAuditService audit,
        ValidationRunner<LoginCommand> loginValidator,
        ValidationRunner<CreateUserAccountCommand> createValidator,
        ValidationRunner<ChangePasswordCommand> changePasswordValidator,
        IAppLogger<AuthService> logger)
    {
        _users = users;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _authorization = authorization;
        _license = license;
        _audit = audit;
        _loginValidator = loginValidator;
        _createValidator = createValidator;
        _changePasswordValidator = changePasswordValidator;
        _logger = logger;
    }

    public async Task<LoginResultDto> LoginAsync(LoginCommand command, CancellationToken cancellationToken = default)
    {
        _loginValidator.ValidateAndThrow(command);
        var user = await GetLoginUserAsync(command.Username, cancellationToken);
        EnsurePasswordIsValid(command.Password, user);

        user.LastLoginAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.Information($"User '{user.Username}' logged in.");
        await _audit.LogAsync(nameof(UserAccount), user.Id.ToString(), AuditAction.Login, cancellationToken: cancellationToken);
        return ToLoginResult(user);
    }

    public async Task<OperationResult> ChangePasswordAsync(ChangePasswordCommand command, CancellationToken cancellationToken = default)
    {
        _changePasswordValidator.ValidateAndThrow(command);
        var user = await GetUserOrThrowAsync(command.UserAccountId, cancellationToken);
        EnsurePasswordIsValid(command.CurrentPassword, user);

        if (_passwordHasher.VerifyPassword(command.NewPassword, user.PasswordHash, user.PasswordSalt))
        {
            throw new ValidationFailedException(
            [
                new ValidationError(nameof(command.NewPassword), "New password must be different from the current password.")
            ]);
        }

        var password = _passwordHasher.HashPassword(command.NewPassword);
        user.PasswordHash = password.Hash;
        user.PasswordSalt = password.Salt;
        user.MustChangePassword = false;
        user.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync(nameof(UserAccount), user.Id.ToString(), AuditAction.Update, newValues: "Password changed", cancellationToken: cancellationToken);
        _logger.Information($"User '{user.Username}' changed password.");
        return OperationResult.Success("Password changed successfully.");
    }

    public async Task<UserAccountDto> CreateUserAsync(CreateUserAccountCommand command, CancellationToken cancellationToken = default)
    {
        await _authorization.EnsurePermissionAsync(PermissionKeys.UsersWrite, cancellationToken);
        _createValidator.ValidateAndThrow(command);
        await EnsureUsernameIsUniqueAsync(command.Username, cancellationToken);
        await EnsureUserLimitNotExceededAsync(cancellationToken);

        var user = CreateEntity(command);
        await _users.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync(nameof(UserAccount), user.Id.ToString(), AuditAction.Create, newValues: user.Username, cancellationToken: cancellationToken);
        _logger.Information($"User '{user.Username}' created.");
        return user.ToDto();
    }

    public async Task<UserAccountDto> UpdateStatusAsync(UpdateUserAccountStatusCommand command, CancellationToken cancellationToken = default)
    {
        await _authorization.EnsurePermissionAsync(PermissionKeys.UsersWrite, cancellationToken);
        var user = await GetUserOrThrowAsync(command.Id, cancellationToken);
        var oldStatus = user.IsActive;
        user.IsActive = command.IsActive;
        user.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync(
            nameof(UserAccount),
            user.Id.ToString(),
            AuditAction.Update,
            oldValues: oldStatus.ToString(),
            newValues: command.IsActive.ToString(),
            cancellationToken: cancellationToken);
        return user.ToDto();
    }

    public async Task<OperationResult> DeleteAsync(DeleteUserAccountCommand command, CancellationToken cancellationToken = default)
    {
        await _authorization.EnsurePermissionAsync(PermissionKeys.UsersWrite, cancellationToken);
        var user = await GetUserOrThrowAsync(command.Id, cancellationToken);
        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync(nameof(UserAccount), user.Id.ToString(), AuditAction.Delete, cancellationToken: cancellationToken);
        return OperationResult.Success("User account deactivated.");
    }

    public async Task<UserAccountDto> GetByIdAsync(GetUserAccountByIdQuery query, CancellationToken cancellationToken = default)
    {
        await _authorization.EnsurePermissionAsync(PermissionKeys.UsersRead, cancellationToken);
        var user = await GetUserOrThrowAsync(query.Id, cancellationToken);
        return user.ToDto();
    }

    public Task<LoginResultDto> HandleAsync(LoginCommand command, CancellationToken cancellationToken = default) => LoginAsync(command, cancellationToken);
    public Task<OperationResult> HandleAsync(ChangePasswordCommand command, CancellationToken cancellationToken = default) => ChangePasswordAsync(command, cancellationToken);
    public Task<UserAccountDto> HandleAsync(CreateUserAccountCommand command, CancellationToken cancellationToken = default) => CreateUserAsync(command, cancellationToken);
    public Task<UserAccountDto> HandleAsync(UpdateUserAccountStatusCommand command, CancellationToken cancellationToken = default) => UpdateStatusAsync(command, cancellationToken);
    public Task<OperationResult> HandleAsync(DeleteUserAccountCommand command, CancellationToken cancellationToken = default) => DeleteAsync(command, cancellationToken);
    public Task<UserAccountDto> HandleAsync(GetUserAccountByIdQuery query, CancellationToken cancellationToken = default) => GetByIdAsync(query, cancellationToken);

    private async Task<UserAccount> GetLoginUserAsync(string username, CancellationToken cancellationToken)
    {
        return await _users.GetByUsernameAsync(username.Trim(), cancellationToken)
            ?? throw new UnauthorizedAppException();
    }

    private async Task<UserAccount> GetUserOrThrowAsync(long id, CancellationToken cancellationToken)
    {
        return await _users.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(UserAccount), id);
    }

    private void EnsurePasswordIsValid(string password, UserAccount user)
    {
        if (!user.IsActive || !_passwordHasher.VerifyPassword(password, user.PasswordHash, user.PasswordSalt))
        {
            throw new UnauthorizedAppException();
        }
    }

    private async Task EnsureUsernameIsUniqueAsync(string username, CancellationToken cancellationToken)
    {
        if (await _users.GetByUsernameAsync(username.Trim(), cancellationToken) is not null)
        {
            throw new ConflictException($"Username '{username}' already exists.");
        }
    }

    private async Task EnsureUserLimitNotExceededAsync(CancellationToken cancellationToken)
    {
        if (!_license.IsValid)
        {
            throw new ForbiddenException("License is invalid or expired.");
        }

        var activeUsers = await _users.CountActiveAsync(cancellationToken);
        if (activeUsers >= _license.MaxUsers)
        {
            throw new ForbiddenException($"User limit reached ({_license.MaxUsers} users allowed by license).");
        }
    }

    private UserAccount CreateEntity(CreateUserAccountCommand command)
    {
        var password = _passwordHasher.HashPassword(command.Password);
        return new UserAccount
        {
            StaffMemberId = command.StaffMemberId,
            Username = command.Username.Trim(),
            PasswordHash = password.Hash,
            PasswordSalt = password.Salt
        };
    }

    private static LoginResultDto ToLoginResult(UserAccount user)
    {
        var permissions = user.UserRoles
            .SelectMany(role => role.Role?.RolePermissions ?? [])
            .Select(rolePermission => rolePermission.Permission?.PermissionKey)
            .Where(permission => !string.IsNullOrWhiteSpace(permission))
            .Distinct()
            .Cast<string>()
            .ToList();

        return new LoginResultDto(
            user.Id,
            user.Username,
            user.StaffMember?.FullNameArabic ?? user.Username,
            permissions,
            user.MustChangePassword);
    }
}

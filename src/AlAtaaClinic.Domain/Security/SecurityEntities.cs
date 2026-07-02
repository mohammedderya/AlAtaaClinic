using AlAtaaClinic.Domain.Common;
using AlAtaaClinic.Domain.Core;

namespace AlAtaaClinic.Domain.Security;

public sealed class UserAccount : AggregateRoot
{
    public long StaffMemberId { get; set; }
    public string Username { get; set; } = string.Empty;
    public byte[] PasswordHash { get; set; } = [];
    public byte[] PasswordSalt { get; set; } = [];
    public bool IsActive { get; set; } = true;
    public bool MustChangePassword { get; set; }
    public DateTime? LastLoginAt { get; set; }

    public StaffMember? StaffMember { get; set; }
    public List<UserRole> UserRoles { get; set; } = [];
}

public sealed class Role : Entity<long>
{
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }

    public List<UserRole> UserRoles { get; set; } = [];
    public List<RolePermission> RolePermissions { get; set; } = [];
}

public sealed class Permission : Entity<long>
{
    public string PermissionKey { get; set; } = string.Empty;
    public string? Description { get; set; }

    public List<RolePermission> RolePermissions { get; set; } = [];
}

public sealed class UserRole
{
    public long UserAccountId { get; set; }
    public long RoleId { get; set; }

    public UserAccount? UserAccount { get; set; }
    public Role? Role { get; set; }
}

public sealed class RolePermission
{
    public long RoleId { get; set; }
    public long PermissionId { get; set; }

    public Role? Role { get; set; }
    public Permission? Permission { get; set; }
}

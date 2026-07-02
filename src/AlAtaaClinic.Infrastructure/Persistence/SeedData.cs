using AlAtaaClinic.Application.Abstractions.Security;
using AlAtaaClinic.Domain.Core;
using AlAtaaClinic.Domain.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AlAtaaClinic.Infrastructure.Persistence;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ClinicDbContext>();

        var allPermissionKeys = PermissionKeys.All;

        var existingKeys = await context.Permissions.Select(p => p.PermissionKey).ToListAsync();
        var missingKeys = allPermissionKeys.Except(existingKeys).ToList();
        if (missingKeys.Count != 0)
        {
            var newPermissions = missingKeys.Select(key => new Permission
            {
                PermissionKey = key,
                Description = key.Contains("read") ? "Read access" : "Write access"
            }).ToList();
            context.Permissions.AddRange(newPermissions);
            await context.SaveChangesAsync();

            var existingAdminRole = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Admin");
            if (existingAdminRole is not null)
            {
                var adminRolePermissions = newPermissions.Select(p => new RolePermission
                {
                    RoleId = existingAdminRole.Id,
                    PermissionId = p.Id
                }).ToList();
                context.RolePermissions.AddRange(adminRolePermissions);
                await context.SaveChangesAsync();
            }
        }

        var existingAdmin = await context.UserAccounts.FirstOrDefaultAsync(u => u.Username == "admin");
        if (existingAdmin is not null)
        {
            if (existingAdmin.LastLoginAt is null)
            {
                existingAdmin.MustChangePassword = true;
                await context.SaveChangesAsync();
            }

            return;
        }

        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        var branch = await context.Branches.FirstOrDefaultAsync(b => b.BranchCode == "HQ");
        if (branch is null)
        {
            branch = new Branch
            {
                BranchCode = "HQ",
                ArabicName = "الفرع الرئيسي",
                EnglishName = "Main Branch",
                PhoneNumber = "0123456789",
                AddressLine = "شارع الملك فهد",
                IsActive = true
            };
            context.Branches.Add(branch);
            await context.SaveChangesAsync();
        }

        var staffMember = await context.StaffMembers.FirstOrDefaultAsync(s => s.StaffCode == "ADMIN001");
        if (staffMember is null)
        {
            staffMember = new StaffMember
            {
                BranchId = branch.Id,
                StaffCode = "ADMIN001",
                FullNameArabic = "مدير النظام",
                FullNameEnglish = "System Admin",
                JobTitle = "مدير عام",
                IsActive = true
            };
            context.StaffMembers.Add(staffMember);
            await context.SaveChangesAsync();
        }

        var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Admin");
        if (adminRole is null)
        {
            var permissions = await context.Permissions
                .Where(p => allPermissionKeys.Contains(p.PermissionKey))
                .ToListAsync();

            adminRole = new Role
            {
                RoleName = "Admin",
                Description = "Full system access"
            };
            context.Roles.Add(adminRole);
            await context.SaveChangesAsync();

            var rolePermissions = permissions.Select(p => new RolePermission
            {
                RoleId = adminRole.Id,
                PermissionId = p.Id
            }).ToList();

            context.RolePermissions.AddRange(rolePermissions);
            await context.SaveChangesAsync();
        }

        var password = passwordHasher.HashPassword("admin123");
        var adminUser = new UserAccount
        {
            StaffMemberId = staffMember.Id,
            Username = "admin",
            PasswordHash = password.Hash,
            PasswordSalt = password.Salt,
            IsActive = true,
            MustChangePassword = true
        };
        context.UserAccounts.Add(adminUser);
        await context.SaveChangesAsync();

        context.UserRoles.Add(new UserRole
        {
            UserAccountId = adminUser.Id,
            RoleId = adminRole.Id
        });
        await context.SaveChangesAsync();
    }
}

using Acme.Admin.Api.Data;
using Acme.Admin.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Acme.Admin.Api.Security;

public sealed class SecuritySeedHostedService(IServiceProvider serviceProvider, ILogger<SecuritySeedHostedService> logger) : IHostedService
{
    private static readonly IReadOnlyDictionary<string, LegacyRoleTemplate> LegacyRoleTemplates =
        new Dictionary<string, LegacyRoleTemplate>(StringComparer.Ordinal)
        {
            ["ADMIN"] = new LegacyRoleTemplate(
                "Full system administrator with canonical permissions",
                [
                    "users.read", "users.write",
                    "roles.read", "roles.write",
                    "taxes.read", "taxes.write",
                    "orders.read", "orders.write",
                    "procurement.read", "procurement.write",
                    "finance.read", "finance.write",
                    "hr.read", "hr.write",
                    "catalog.read", "catalog.write",
                    "billing.read", "billing.create", "billing.pay"
                ]),
            ["MANAGER"] = new LegacyRoleTemplate(
                "Manager with read/write access to business data",
                [
                    "catalog.read", "catalog.write",
                    "taxes.read", "taxes.write",
                    "orders.read", "orders.write",
                    "procurement.read", "procurement.write",
                    "finance.read", "finance.write",
                    "hr.read", "hr.write",
                    "billing.read", "billing.create", "billing.pay"
                ]),
            ["VIEWER"] = new LegacyRoleTemplate(
                "Read-only access to business data",
                [
                    "catalog.read",
                    "taxes.read",
                    "orders.read",
                    "procurement.read",
                    "finance.read",
                    "hr.read",
                    "billing.read",
                    "users.read",
                    "roles.read"
                ])
        };

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = serviceProvider.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<AcmeDbContext>();

            await SeedPermissionsAsync(db, cancellationToken);
            await SeedSuperAdminRoleAsync(db, cancellationToken);
            await SeedLegacyTemplateRolesAsync(db, cancellationToken);
            await SeedAdminUserAsync(db, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Security seed skipped");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static async Task SeedPermissionsAsync(AcmeDbContext db, CancellationToken ct)
    {
        foreach (var spec in PermissionCatalog.Specs())
        {
            var exists = await db.AuthPermissions.AnyAsync(x => x.Code == spec.Code, ct);
            if (exists)
            {
                continue;
            }

            db.AuthPermissions.Add(new AuthPermissionEntity
            {
                Id = Guid.NewGuid(),
                Code = spec.Code,
                Description = spec.Description
            });
        }

        await db.SaveChangesAsync(ct);
    }

    private static async Task SeedLegacyTemplateRolesAsync(AcmeDbContext db, CancellationToken ct)
    {
        var canonicalCodes = PermissionCatalog.AllCodes();
        var canonicalPermissions = (await db.AuthPermissions
                .Where(x => canonicalCodes.Contains(x.Code))
                .ToListAsync(ct))
            .ToDictionary(x => x.Code, x => x, StringComparer.Ordinal);

        foreach (var (roleName, template) in LegacyRoleTemplates)
        {
            var role = await db.AuthRoles
                .Include(x => x.Permissions)
                .FirstOrDefaultAsync(x => x.Name == roleName, ct);

            if (role is null)
            {
                role = new AuthRoleEntity
                {
                    Id = Guid.NewGuid(),
                    Name = roleName,
                    Description = template.Description
                };
                db.AuthRoles.Add(role);
            }
            else if (string.IsNullOrWhiteSpace(role.Description))
            {
                role.Description = template.Description;
            }

            var templateCodes = template.PermissionCodes.ToHashSet(StringComparer.Ordinal);

            foreach (var existing in role.Permissions.Where(x => !templateCodes.Contains(x.Code)).ToList())
            {
                role.Permissions.Remove(existing);
            }

            foreach (var code in templateCodes)
            {
                if (!canonicalPermissions.TryGetValue(code, out var permission))
                {
                    continue;
                }

                if (!role.Permissions.Any(x => x.Id == permission.Id))
                {
                    role.Permissions.Add(permission);
                }
            }
        }

        await db.SaveChangesAsync(ct);
    }

    private static async Task SeedSuperAdminRoleAsync(AcmeDbContext db, CancellationToken ct)
    {
        var canonicalCodes = PermissionCatalog.AllCodes();

        var role = await db.AuthRoles
            .Include(x => x.Permissions)
            .FirstOrDefaultAsync(x => x.Name == "SuperAdmin", ct);

        var canonicalPermissions = await db.AuthPermissions
            .Where(x => canonicalCodes.Contains(x.Code))
            .ToListAsync(ct);

        if (role is null)
        {
            role = new AuthRoleEntity
            {
                Id = Guid.NewGuid(),
                Name = "SuperAdmin",
                Description = "Full access"
            };
            db.AuthRoles.Add(role);
        }

        foreach (var existing in role.Permissions.Where(x => !canonicalCodes.Contains(x.Code)).ToList())
        {
            role.Permissions.Remove(existing);
        }

        foreach (var permission in canonicalPermissions)
        {
            if (!role.Permissions.Any(x => x.Id == permission.Id))
            {
                role.Permissions.Add(permission);
            }
        }

        await db.SaveChangesAsync(ct);
    }

    private static async Task SeedAdminUserAsync(AcmeDbContext db, CancellationToken ct)
    {
        var admin = await db.AuthUsers
            .Include(x => x.Roles)
            .FirstOrDefaultAsync(x => x.Username == "admin", ct);

        var superAdmin = await db.AuthRoles.FirstOrDefaultAsync(x => x.Name == "SuperAdmin", ct);
        if (superAdmin is null)
        {
            return;
        }

        if (admin is null)
        {
            admin = new AuthUserEntity
            {
                Id = Guid.NewGuid(),
                Email = "admin@acme.local",
                Username = "admin",
                DisplayName = "System Administrator",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                Status = AuthUserStatus.ACTIVE,
                Enabled = true
            };

            admin.Roles.Add(superAdmin);
            db.AuthUsers.Add(admin);
            await db.SaveChangesAsync(ct);
            return;
        }

        if (!admin.Roles.Any(x => x.Id == superAdmin.Id))
        {
            admin.Roles.Add(superAdmin);
            await db.SaveChangesAsync(ct);
        }
    }

    private sealed record LegacyRoleTemplate(string Description, IReadOnlyList<string> PermissionCodes);
}

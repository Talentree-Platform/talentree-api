// Talentree.Repository/Identity/AppIdentityDbContextSeed.cs

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Talentree.Core.Entities.Identity;
using Talentree.Repository.Data;

namespace Talentree.Repository.Data
{
    /// <summary>
    /// Seeds initial identity data (roles and users).
    /// </summary>
    public static class TalentreeContextSeed
    {
        public static async Task SeedAsync(
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager,
            TalentreeDbContext context,
            ILogger logger)
        {
            try
            {
                // ═══════════════════════════════════════════════════════════
                // STEP 1: Seed Roles
                // ═══════════════════════════════════════════════════════════
                await SeedRolesAsync(roleManager, logger);

                // ═══════════════════════════════════════════════════════════
                // STEP 2: Seed Admin User
                // ═══════════════════════════════════════════════════════════
                await SeedAdminUserAsync(userManager, logger);

                // ═══════════════════════════════════════════════════════════
                // STEP 3: Seed Customers
                // ═══════════════════════════════════════════════════════════
                await SeedCustomersAsync(userManager, logger);

                logger.LogInformation("✅ Identity seeding completed successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ An error occurred during identity seeding");
            }
        }

        // ── Fixed customer GUIDs ─────────────────────────────────────────
        // Pinned so jsonSeed/ FK references (LoginHistories, ProductReviews,
        // etc.) always resolve regardless of which machine seeds the DB.
        // Mirrors the BusinessOwnerSeed pattern for BO accounts.
        public const string MonaId   = "b1402433-d75c-4a91-b4d1-d1f92abee781";
        public const string KhaledId = "c34f2835-56c0-4eef-8291-93f70b385ae1";
        public const string NourId   = "b65f6a9d-b5c0-41d8-93de-59b43a0e7b42";
        public const string YasserId = "05515c97-a07e-4e62-901d-7cbe371de8d7";
        public const string LailaId  = "791f91bd-8afb-4399-9cf5-26ce52b801d7";
        public const string KarimId  = "3a9d3797-1d5c-4dc0-b4cb-bb33125c80a7";

        // ═══════════════════════════════════════════════════════════
        // SEED ROLES
        // ═══════════════════════════════════════════════════════════
        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager, ILogger logger)
        {
            var roles = new[] { "Admin", "BusinessOwner", "Customer" };

            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    var role = new IdentityRole(roleName);
                    await roleManager.CreateAsync(role);
                    logger.LogInformation($"✅ Role created: {roleName}");
                }
            }
        }

        // ═══════════════════════════════════════════════════════════
        // SEED ADMIN USER
        // ═══════════════════════════════════════════════════════════
        private static async Task SeedAdminUserAsync(UserManager<AppUser> userManager, ILogger logger)
        {
            const string adminEmail = "admin@talentree.com";

            if (await userManager.FindByEmailAsync(adminEmail) != null)
                return;

            var admin = new AppUser
            {
                DisplayName    = "System Administrator",
                Email          = adminEmail,
                UserName       = adminEmail,
                PhoneNumber    = "01000000000",
                EmailConfirmed = true,
                IsActive       = true,
                CreatedAt      = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(admin, "Admin@123456");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Admin");
                logger.LogInformation($"✅ Admin user created: {adminEmail}");
            }
            else
            {
                logger.LogError($"❌ Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }

        // ═══════════════════════════════════════════════════════════
        // SEED CUSTOMERS
        // Each customer is created individually so we can set a fixed Id
        // that matches the UUIDs already baked into the jsonSeed/ files.
        // ═══════════════════════════════════════════════════════════
        private static async Task SeedCustomersAsync(UserManager<AppUser> userManager, ILogger logger)
        {
            await CreateCustomerAsync(userManager, logger, MonaId,   "Mona Ahmed",     "mona.ahmed@example.com",     "01234567111");
            await CreateCustomerAsync(userManager, logger, KhaledId, "Khaled Mahmoud", "khaled.mahmoud@example.com", "01234567222");
            await CreateCustomerAsync(userManager, logger, NourId,   "Nour Hassan",    "nour.hassan@example.com",    "01234567333");
            await CreateCustomerAsync(userManager, logger, YasserId, "Yasser Ali",     "yasser.ali@example.com",     "01234567444");
            await CreateCustomerAsync(userManager, logger, LailaId,  "Laila Mohamed",  "laila.mohamed@example.com",  "01234567555");
            await CreateCustomerAsync(userManager, logger, KarimId,  "Karim Hassan",   "karim.hassan@example.com",   "01234567666");
        }

        private static async Task CreateCustomerAsync(
            UserManager<AppUser> userManager,
            ILogger logger,
            string fixedId,
            string displayName,
            string email,
            string phone)
        {
            var existingUser = await userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                if (existingUser.Id == fixedId)
                    return;

                logger.LogWarning($"[TalentreeContextSeed] Customer {email} exists with dynamic/old ID {existingUser.Id}. Recreating with fixed ID {fixedId}...");
                var deleteResult = await userManager.DeleteAsync(existingUser);
                if (!deleteResult.Succeeded)
                {
                    logger.LogError($"[TalentreeContextSeed] Failed to delete old customer {email}: {string.Join(", ", deleteResult.Errors.Select(e => e.Description))}");
                    return;
                }
            }

            var user = new AppUser
            {
                Id             = fixedId,
                DisplayName    = displayName,
                Email          = email,
                UserName       = email,
                PhoneNumber    = phone,
                EmailConfirmed = true,
                IsActive       = true,
                CreatedAt      = DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 30))
            };

            var result = await userManager.CreateAsync(user, "Test@123456");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "Customer");
                logger.LogInformation($"✅ Customer created: {email} [{fixedId}]");
            }
            else
            {
                logger.LogError($"❌ Failed to create customer {email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
    }
}
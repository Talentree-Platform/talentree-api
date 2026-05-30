// Talentree.Repository/Identity/AppIdentityDbContextSeed.cs

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Talentree.Core.Entities.Identity;
using Talentree.Core.Enums;
using Talentree.Repository.Data;

namespace Talentree.Repository.Data
{
    /// <summary>
    /// Seeds initial identity data (roles and users)
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
            var adminEmail = "admin@talentree.com";

            var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
            if (existingAdmin == null)
            {
                var admin = new AppUser
                {
                    DisplayName = "System Administrator",
                    Email = adminEmail,
                    UserName = adminEmail,
                    PhoneNumber = "01000000000",
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(admin, "Admin@123456");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                    logger.LogInformation($" Admin user created: {adminEmail}");
                }
                else
                {
                    logger.LogError($" Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }

        
        // ═══════════════════════════════════════════════════════════
        // SEED CUSTOMERS
        // ═══════════════════════════════════════════════════════════
        private static async Task SeedCustomersAsync(UserManager<AppUser> userManager, ILogger logger)
        {
            var customers = new[]
            {
                new
                {
                    DisplayName = "Mona Ahmed",
                    Email = "mona.ahmed@example.com",
                    PhoneNumber = "01234567111",
                    Password = "Test@123456"
                },
                new
                {
                    DisplayName = "Khaled Mahmoud",
                    Email = "khaled.mahmoud@example.com",
                    PhoneNumber = "01234567222",
                    Password = "Test@123456"
                },
                new
                {
                    DisplayName = "Nour Hassan",
                    Email = "nour.hassan@example.com",
                    PhoneNumber = "01234567333",
                    Password = "Test@123456"
                },
                new
                {
                    DisplayName = "Yasser Ali",
                    Email = "yasser.ali@example.com",
                    PhoneNumber = "01234567444",
                    Password = "Test@123456"
                },
                new
                {
                    DisplayName = "Laila Mohamed",
                    Email = "laila.mohamed@example.com",
                    PhoneNumber = "01234567555",
                    Password = "Test@123456"
                }
            };

            foreach (var customer in customers)
            {
                var existingUser = await userManager.FindByEmailAsync(customer.Email);
                if (existingUser == null)
                {
                    var user = new AppUser
                    {
                        DisplayName = customer.DisplayName,
                        Email = customer.Email,
                        UserName = customer.Email,
                        PhoneNumber = customer.PhoneNumber,
                        EmailConfirmed = true,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 30))
                    };

                    var result = await userManager.CreateAsync(user, customer.Password);

                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, "Customer");
                        logger.LogInformation($"✅ Customer created: {customer.Email}");
                    }
                }
            }
        }
    }
}